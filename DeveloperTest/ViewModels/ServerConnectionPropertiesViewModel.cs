﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using CommonServiceLocator;
using DeveloperTest.ConnectionService;
using DeveloperTest.EmailService;
using DeveloperTest.MessageBus;
using GalaSoft.MvvmLight.CommandWpf;
using Ninject.Extensions.Logging;
using MvvmDialogs;
using DeveloperTest.Utils.Extensions;
using DeveloperTest.Utils.WPF;
using DeveloperTest.ViewModels.Popups;

namespace DeveloperTest.ViewModels
{
    public class ServerConnectionPropertiesViewModel : CommonViewModel, IDataErrorInfo
    {
        #region Fields

        private string _selectedProtocol;
        private string _selectedEncryptionType;
        private string _serverName;
        private string _port;
        private string _username;
        private string _password;
        private RelayCommand _startCommand;
        private bool _btnStartHasBeenClicked;
        private bool _isProcessing;
        private string _messageCurrentOperation;
        private readonly IEmailConnectionUtils _connectionUtils;
        private readonly IDialogService _dialogService;
        #endregion

        #region Properties
        public List<string> ProtocolsLst { get; set; }
        public List<string> EncryptionTypesLst { get; set; }

        public bool BtnStartHasBeenUsed
        {
            get => _btnStartHasBeenClicked;
            set
            {
                Set(() => BtnStartHasBeenUsed, ref _btnStartHasBeenClicked, value);
            }
        }

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                Set(() => IsProcessing, ref _isProcessing, value);
            }
        }

        public string SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                Set(() => SelectedProtocol, ref _selectedProtocol, value);
                if(_selectedEncryptionType != null)
                    Port = ConnectionPortUtils.GetDefaultPortForProtocol((Protocols)Enum.Parse(typeof(Protocols), value), (EncryptionTypes)Enum.Parse(typeof(EncryptionTypes), _selectedEncryptionType));
            }
        }

        public string SelectedEncryptionType
        {
            get => _selectedEncryptionType;
            set
            {
                Set(() => SelectedEncryptionType, ref _selectedEncryptionType, value);
                if(_selectedProtocol != null)
                    Port = ConnectionPortUtils.GetDefaultPortForProtocol((Protocols)Enum.Parse(typeof(Protocols), _selectedProtocol), (EncryptionTypes)Enum.Parse(typeof(EncryptionTypes), value));
            }
        }

        public string ServerName
        {
            get => _serverName;
            set
            {
                Set(() => ServerName, ref _serverName, value);
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        public string Port
        {
            get => _port;
            set
            {
                Set(() => Port, ref _port, value);
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                Set(() => Username, ref _username, value);
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        // ReSharper disable once UnusedMember.Global
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                StartCommand.RaiseCanExecuteChanged();
            }
        }

        public string MessageCurrentOperation
        {
            get => _messageCurrentOperation;
            set
            {
                Set(() => MessageCurrentOperation, ref _messageCurrentOperation, value);
            }
        }

        private bool CanStartRetrieveEmails()
        {
            return !string.IsNullOrEmpty(_serverName) &&
                   !string.IsNullOrEmpty(_username) &&
                   !string.IsNullOrEmpty(_password) &&
                   string.IsNullOrEmpty(Error) &&
                   !IsProcessing && !BtnStartHasBeenUsed;
        }

        #endregion

        #region Ctor

        public ServerConnectionPropertiesViewModel(ILogger logger) : base(logger)
        {
            _connectionUtils = ServiceLocator.Current.GetInstance<IEmailConnectionUtils>();
            _dialogService = ServiceLocator.Current.GetInstance<IDialogService>();
        }

        #endregion

        #region Commands

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new RelayCommand(async () =>
                {
                    IsProcessing = true;
                    RaisePropertyChanged(() => IsProcessing);

                    var connectionDataDescriptor =
                        ServiceLocator.Current.GetInstance<IEmailConnectionDescriptorInstance>();

                    var cd = new ConnectionDescriptor
                    {
                        EncryptionType = (EncryptionTypes) Enum.Parse(typeof(EncryptionTypes), _selectedEncryptionType),
                        MailProtocol = (Protocols) Enum.Parse(typeof(Protocols), _selectedProtocol),
                        Port = Convert.ToInt32(Port),
                        Server = ServerName,
                        Username = Username,
                        Password = Password
                    };
                    //save these data for the running program instance as we will need these for later
                    connectionDataDescriptor.SetConnectionData(cd);

                    connectionDataDescriptor.ConnectionsList = _connectionUtils.CreateConnections();

                    #region Connection stage && Authentication stage

                    MessageCurrentOperation = "Connecting and authenticating...";

                    bool connectionFailed = false;
                    string errorConnectionMessage = null;
                    try
                    {
                        await _connectionUtils.ConnectAndAuthenticateAsync(connectionDataDescriptor.ConnectionsList);
                    }
                    catch (Exception e)
                    {
                        connectionFailed = true;
                        errorConnectionMessage = e.Message;
                    }
                    finally
                    {
                        if (connectionFailed)
                        {
                            var errorPopupViewModelConn = new ErrorPopupViewModel(Logger)
                            {
                                Message = $"Could not connect/authenticate,  host {cd.Server}:{cd.Port}" + "\r\n" + errorConnectionMessage
                            };

                            await _dialogService.ShowDialogAsync(this, errorPopupViewModelConn);
                        }
                    }

                    if (connectionFailed)
                    {
                        await _connectionUtils.DisconnectAsync(connectionDataDescriptor.ConnectionsList);
                        IsProcessing = false;
                        RaisePropertyChanged(() => IsProcessing);
                        return;
                    }

                    #endregion

                    #region Download process

                    MessageCurrentOperation = "Downloading emails...";

                    MessengerInstance.Send(new StartScanEmailMessage());

                    #endregion

                    //make the start button not click-able anymore for this test
                    BtnStartHasBeenUsed = true;
                    RaisePropertyChanged(() => BtnStartHasBeenUsed);

                    IsProcessing = false;
                    RaisePropertyChanged(() => IsProcessing);
                }, CanStartRetrieveEmails));
            }
        }

        #endregion


        #region overrides

        protected override Task ExecuteOnLoad()
        {
            ProtocolsLst = new List<string>();
            EncryptionTypesLst = new List<string>();

            foreach (var protocol in Enum.GetValues(typeof(Protocols)))
            {
                ProtocolsLst.Add(protocol.ToString());
            }
            foreach (var encryptionType in Enum.GetValues(typeof(EncryptionTypes)))
            {
                EncryptionTypesLst.Add(encryptionType.ToString());
            }

            RaisePropertyChanged(() => ProtocolsLst);
            RaisePropertyChanged(() => EncryptionTypesLst);

            return base.ExecuteOnLoad();
        }

        #endregion

        #region IDataErrorInfo
        public string this[string columnName]
        {
            get
            {
                Error = string.Empty;
                switch (columnName)
                {
                    case nameof(Port):
                        if (_port == null) break;
                        if (!int.TryParse(_port, out int port))
                            Error = "Port is not a valid number!";
                        break;
                }

                return Error;
            }
        }

        public string Error { get; set; }

        #endregion
    }
}
