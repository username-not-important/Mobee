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
using Mobee.Client.WPF.Stores;
using Mobee.Client.WPF.ViewModels;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for LanguageWindow.xaml
    /// </summary>
    public partial class LanguageWindow : Window
    {
        public LanguageWindow()
        {
            InitializeComponent();
        }
        
        private void saveLanguageSettings(string culture)
        {
            Properties.Settings.Default.CULTURE = culture;

            Properties.Settings.Default.Save();
        }

        private void _LanguageButtonClick(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement)?.Tag?.ToString() ?? "en-US";

            saveLanguageSettings(tag);

            App.Restart();
        }
    }
}
