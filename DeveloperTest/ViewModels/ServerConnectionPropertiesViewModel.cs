using System;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Ninject.Extensions.Logging;

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

        #endregion

        #region Properties

        public string SelectedProtocol
        {
            get => _selectedProtocol;
            set
            {
                Set(() => SelectedProtocol, ref _selectedProtocol, value);
            }
        }

        public string SelectedEncryptionType
        {
            get => _selectedEncryptionType;
            set
            {
                Set(() => SelectedEncryptionType, ref _selectedEncryptionType, value);
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

        public RelayCommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new RelayCommand(() =>
                {

                },CanStartRetrieveEmails));
            }
        }

        private bool CanStartRetrieveEmails()
        {
            return !string.IsNullOrEmpty(_serverName) &&
                   !string.IsNullOrEmpty(_username) &&
                   !string.IsNullOrEmpty(_password) &&
                   string.IsNullOrEmpty(Error);
        }

        #endregion

        #region Ctor

        public ServerConnectionPropertiesViewModel(ILogger logger) : base(logger)
        {
            Port = "913";
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
