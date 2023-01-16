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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HubConnection connection;
        private string Id = new Random().Next(1, 1000).ToString("D4");
        private bool Status = false;

        public MainWindow()
        {
            InitializeComponent();
            
            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7016/PlayersHub")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await connection.StartAsync();
            };
        }
        
        private async void _Connect_Click(object sender, RoutedEventArgs e)
        {
            connection.On<string, bool>("PlaybackToggled", (user, status) =>
            {
                Status = status;

                this.Dispatcher.Invoke(() =>
                {
                    _StatusText.Text = status ? "Playing" : "Stopped";
                });
            });

            try
            {
                await connection.StartAsync();

                _Connect_Button.IsEnabled = false;
                _Toggle_Button.IsEnabled = true;
            }
            catch (Exception ex)
            {
                _Connect_Button.IsEnabled = true;
                _Toggle_Button.IsEnabled = false;
            }
        }

        private async void _Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Status = !Status;

                await connection.InvokeAsync("TogglePlayback", 
                    $"Player {Id}", Status);
            }
            catch (Exception ex)
            {         
                _Connect_Button.IsEnabled = true;
                _Toggle_Button.IsEnabled = false;
            }
        }
    }
}
