using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading;
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
using FlyleafLib.MediaPlayer;
using FlyleafLib;
using Microsoft.AspNetCore.SignalR.Client;
using Mobee.Client.WPF.Data;
using Mobee.Client.WPF.ViewModels;
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

        public MainWindowViewModel ViewModel { get; set; } = new();

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

            ViewModel.Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        private long _lastCurTime = 0;
        private void PlayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var _last = _lastCurTime;

            _lastCurTime = ViewModel.Player.CurTime;

            if (e.PropertyName == "CurTime")
            {
                if (Math.Abs(_last - ViewModel.Player.CurTime) > 10000 * 1000)
                {
                    ViewModel.ChatViewModel.Messages.Add(new ChatMessage("Seeked", false, true));
                }
            }
        }

        private async void _Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.StartAsync();

                _Connect_Button.IsEnabled = false;
                //_Toggle_Button.IsEnabled = true;
                ViewModel.Player.Open(@"D:\Documents\Downloaded Videos\Symphonies\Mahler Symphony 2 - Janson.mkv");
                ViewModel.Player.Pause();
            }
            catch (Exception ex)
            {
                _Connect_Button.IsEnabled = true;
                //_Toggle_Button.IsEnabled = false;
            }
        }

        private async void _Toggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Status = !Status;

                var position = (int)ViewModel.Player.CurTime;
                var toggleTask = Hub.TogglePlayback($"Player {Id}", Status, position);

                if (Status)
                {
                    ViewModel.Player.Play();
                }
                else
                {
                    ViewModel.Player.Pause();
                }

                await toggleTask;
            }
            catch (Exception ex)
            {         
                _Connect_Button.IsEnabled = true;
                //_Toggle_Button.IsEnabled = false;
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
                    ViewModel.ChatViewModel.Messages.Add(new ChatMessage($"me: {message}", true));
                });

                _MessageTextbox.Text = "";
            }
            catch (Exception ex)
            {
                
            }
        }

        public async Task PlaybackToggled(string user, bool isPlaying, long position)
        {
            Status = isPlaying;

            await this.Dispatcher.InvokeAsync(() =>
            {
                string action = isPlaying ? "Resumed" : "Paused";
                
                ViewModel.ChatViewModel.Messages.Add(new ChatMessage($"Playback {action} at {TimeSpan.FromMilliseconds(position/10000):g}", false, true));
                
                ViewModel.Player.CurTime = position;

                if (isPlaying)
                    ViewModel.Player.Play();
                else
                    ViewModel.Player.Pause();
                //_StatusText.Text = isPlaying ? "Playing" : "Stopped";
            });
        }

        public async Task ReceiveMessage(string from, string message)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                ViewModel.ChatViewModel.Messages.Add(new ChatMessage($"{from}: {message}"));
            });
        }
        
    }
}
