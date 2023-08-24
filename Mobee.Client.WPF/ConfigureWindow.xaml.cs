using System;
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
using System.Windows.Shapes;
using Microsoft.Win32;
using Mobee.Client.WPF.IoC;
using Mobee.Client.WPF.Properties;
using Mobee.Client.WPF.Stores;
using Mobee.Client.WPF.ViewModels;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for ConfigureWindow.xaml
    /// </summary>
    public partial class ConfigureWindow : Window
    {
        public ConfigurationViewModel ViewModel { get; set; }
        public ConfigurationStore ConfigurationStore { get; set; }

        public ConfigureWindow(IAbstractFactory<ConfigurationViewModel> viewModelFactory, ConfigurationStore configurationStore)
        {
            ViewModel = viewModelFactory.Create();
            ConfigurationStore = configurationStore;
            
            loadConfiguration();

            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                var path = dialog.FileName;

                ViewModel.FilePath = path;
            }
        }

        private async void Launch_Click(object sender, RoutedEventArgs e)
        {
            saveConfiguration();

            var mainWindow = ((App)Application.Current).GetRequiredService<MainWindow>();
            mainWindow.Show();

            Close();
        }
        
        private void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.CULTURE = null;
            Settings.Default.Save();

            App.Restart();
        }

        private void saveConfiguration()
        {
            Properties.Settings.Default.SERVER_BASEURI = ConfigurationStore.ServerAddress = ViewModel.ServerAddress;
            Properties.Settings.Default.LAST_MEDIA_FILE = ConfigurationStore.FilePath = ViewModel.FilePath;
            Properties.Settings.Default.LAST_USERNAME = ConfigurationStore.UserName = ViewModel.UserName;
            Properties.Settings.Default.LAST_GROUPNAME = ConfigurationStore.GroupName = ViewModel.GroupName;

            Properties.Settings.Default.Save();
        }

        private void loadConfiguration()
        {
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SERVER_BASEURI))
                ViewModel.ServerAddress = Properties.Settings.Default.SERVER_BASEURI;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LAST_MEDIA_FILE))
                ViewModel.FilePath = Properties.Settings.Default.LAST_MEDIA_FILE;
            
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LAST_USERNAME))
                ViewModel.UserName = Properties.Settings.Default.LAST_USERNAME;

            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.LAST_GROUPNAME))
                ViewModel.GroupName = Properties.Settings.Default.LAST_GROUPNAME;
        }

    }
}
