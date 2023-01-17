using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Mobee.Common;
using TypedSignalR.Client;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPlayerClient
    {
        private IPlayerHub Hub { get; }

        private HubConnection connection;
        private string Id = new Random().Next(1, 1000).ToString("D4");

        private bool Status = false;

        public ObservableCollection<HubMessage> Messages { get; set; } = new();

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

            Hub = connection.CreateHubProxy<IPlayerHub>();
            var subscription = connection.Register<IPlayerClient>(this);
        }
        
        private async void _Connect_Click(object sender, RoutedEventArgs e)
        {
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

                await Hub.TogglePlayback($"Player {Id}", Status);
                //await connection.InvokeAsync("TogglePlayback",  $"Player {Id}", Status);
            }
            catch (Exception ex)
            {         
                _Connect_Button.IsEnabled = true;
                _Toggle_Button.IsEnabled = false;
            }
        }

        private async void _SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            var message = _MessageTextbox.Text;
            if (string.IsNullOrWhiteSpace(message))
                return;

            try
            {
                await Hub.SendMessage(Id, message);
                //await connection.InvokeAsync("SendMessage", Id, message);

                this.Dispatcher.Invoke(() =>
                {
                    Messages.Add(new HubMessage($"me: {message}", true));
                });

                _MessageTextbox.Text = "";
            }
            catch (Exception ex)
            {
                
            }
        }

        public async Task PlaybackToggled(string user, bool isPlaying)
        {
            Status = isPlaying;

            await this.Dispatcher.InvokeAsync(() =>
            {
                _StatusText.Text = isPlaying ? "Playing" : "Stopped";
            });
        }

        public async Task ReceiveMessage(string from, string message)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                Messages.Add(new HubMessage($"{from}: {message}"));
            });
        }
    }
    
    public class HubMessage
    {
        public HubMessage(string message, bool isSelf = false)
        {
            Message = message;
            IsSelf = isSelf;
        }

        public bool IsSelf { get; set; }

        public string Message { get; set; }
    }

}
