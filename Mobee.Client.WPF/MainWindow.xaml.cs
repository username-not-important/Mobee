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
using Mobee.Client.WPF.IoC;
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
        private IPlayerHub Hub { get; set; }

        private HubConnection connection;
        private string Id = new Random().Next(1, 1000).ToString("D4");

        private bool Status = false;
        private bool SyncLock = false;

        public MainWindowViewModel ViewModel { get; set; }

        public MainWindow(IAbstractFactory<MainWindowViewModel> viewModelFactory)
        {
            ViewModel = viewModelFactory.Create();

            InitializeComponent();
            InitializeHub();
            
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void InitializeHub()
        {
            var baseUri = Properties.Settings.Default.SERVER_BASEURI;

            if (string.IsNullOrWhiteSpace(baseUri) || !baseUri.StartsWith("https://"))
                return;
            
            baseUri = baseUri.TrimEnd('/');
            //https://localhost:7016

            connection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}/PlayersHub")
                .Build();

            connection.Closed += async (error) =>
            {
                //TODO: test for dispatcher issues
                ViewModel.ConnectionViewModel.IsConnected = false;

                await Task.Delay(new Random().Next(0,5) * 1000);
                await connection.StartAsync();
            };

            Hub = connection.CreateHubProxy<IPlayerHub>();
            var subscription = connection.Register<IPlayerClient>(this);

            ViewModel.Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        private void ReleaseSyncLock()
        {
            Task.Run(() =>
            {
                Thread.Sleep(500);
                SyncLock = false;
            });
        }

        private long _lastCurTime = 0;
        private async void PlayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (SyncLock)
                return;

            var _last = _lastCurTime;

            _lastCurTime = ViewModel.Player.CurTime;

            if (e.PropertyName == "CurTime")
            {
                if (Math.Abs(_last - ViewModel.Player.CurTime) > 50000 * 1000)
                {
                    ViewModel.ChatViewModel.Messages.Add(new ChatMessage("Seeked", false, true));

                    await onSeeked();
                }
            }
            else if (e.PropertyName == "Status")
            {
                await onPlaybackStatusChanged();
            }
        }

        private async void _Connect_Click(object sender, RoutedEventArgs e)
        {
            await Connect();
        }

        private async Task Connect()
        {
            try
            {
                await connection.StartAsync();

                ViewModel.ConnectionViewModel.IsConnected = true;
                ViewModel.Player.Open(Properties.Settings.Default.LAST_MEDIA_FILE);
                ViewModel.Player.Pause();
            }
            catch (Exception ex)
            {
                ViewModel.ConnectionViewModel.IsConnected = false;
            }
        }

        private async Task onPlaybackStatusChanged()
        {
            try
            {
                var newStatus = ViewModel.Player.Status == FlyleafLib.MediaPlayer.Status.Playing;
                if (Status == newStatus)
                    return;

                Status = newStatus;

                var position = ViewModel.Player.CurTime;
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
                ViewModel.ConnectionViewModel.IsConnected = false;
            }
        }

        private async Task onSeeked()
        {
            try
            {
                SyncLock = true;

                var position = ViewModel.Player.CurTime;
                var toggleTask = Hub.TogglePlayback($"Player {Id}", Status, position);

                await toggleTask;

                ReleaseSyncLock();
            }
            catch (Exception e)
            {
                ViewModel.ConnectionViewModel.IsConnected = false;

                ReleaseSyncLock();
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

        #region IPlaybackClient (Receiving)

        public async Task PlaybackToggled(string user, bool isPlaying, long position)
        {
            if (SyncLock)
                return;

            if (isPlaying && position < 0)
                return;

            SyncLock = true;

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
                
                ReleaseSyncLock();
            });
        }

        public async Task ReceiveMessage(string from, string message)
        {
            await this.Dispatcher.InvokeAsync(() =>
            {
                ViewModel.ChatViewModel.Messages.Add(new ChatMessage($"{from}: {message}"));
            });
        }

        #endregion
        
    }
}
