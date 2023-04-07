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
using Mobee.Client.WPF.ViewModels;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for ConfigureWindow.xaml
    /// </summary>
    public partial class ConfigureWindow : Window
    {
        public ConfigurationViewModel ViewModel { get; set; }

        public ConfigureWindow(IAbstractFactory<ConfigurationViewModel> viewModelFactory)
        {
            ViewModel = viewModelFactory.Create();

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

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = ((App)Application.Current).GetRequiredService<MainWindow>();
            mainWindow.Show();

            Close();
        }
    }
}
