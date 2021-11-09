﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DeveloperTest.Views
{
    /// <summary>
    /// Interaction logic for ServerConnectionPropertiesView.xaml
    /// </summary>
    public partial class ServerConnectionPropertiesView : UserControl
    {
        public ServerConnectionPropertiesView()
        {
            InitializeComponent();
            Loaded += ServerConnectionPropertiesView_Loaded;
        }

        private void ServerConnectionPropertiesView_Loaded(object sender, RoutedEventArgs e)
        {
            ((dynamic) DataContext).Password = "YjpkhPe5HxP5hGD";
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext != null)
                ((dynamic)DataContext).Password = ((PasswordBox)sender).Password;
        }
    }
}
