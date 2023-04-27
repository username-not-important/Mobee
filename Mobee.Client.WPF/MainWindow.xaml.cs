using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlyleafLib.MediaPlayer;
using FlyleafLib;
using FlyleafLib.Controls.WPF;
using Microsoft.AspNetCore.SignalR.Client;
using Mobee.Client.WPF.Data;
using Mobee.Client.WPF.IoC;
using Mobee.Client.WPF.Stores;
using Mobee.Client.WPF.ViewModels;
using Mobee.Common;
using TypedSignalR.Client;
using Mobee.Client.WPF.Utilities;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IPlayerClient
    {
        private IPlayerHub Hub { get; set; } = null!;

        private HubConnection connection = null!;

        private bool Status = false;
        private bool SyncLock = false;

        public MainWindowViewModel ViewModel { get; set; }
        public ChatViewModel ChatViewModel { get; }
        public ConnectionViewModel ConnectionViewModel { get; }
        public ConfigurationStore ConfigurationStore { get; set; }

        public MainWindow(ConfigurationStore configurationStore)
        {
            ViewModel = App.VMLocator<MainWindowViewModel>();
            ChatViewModel = App.VMLocator<ChatViewModel>();
            ConnectionViewModel = App.VMLocator<ConnectionViewModel>();
            
            ConfigurationStore = configurationStore;

            InitializeComponent();
            InitializeChat();
            InitializeHub();
            
            Loaded += OnLoaded;
        }

        private void CleanupMessages()
        {
            var collection = ChatViewModel.Messages;

            if (collection.Count > 10)
            {
                var removeList = collection.Take(5).Where(chatMessage => chatMessage.IsBroadcast).ToList();

                foreach (var chatMessage in removeList)
                {
                    collection.Remove(chatMessage);
                }
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await Connect();

            InitializePlayer();
        }

        private void InitializePlayer()
        {
            var ratio = ViewModel.Player.Video.AspectRatio;
            Height = ActualWidth / ratio.Value + 30;

            FlyleafMe.SelectedTheme = FlyleafMe.UIConfig.Themes.FirstOrDefault(x => x.Name == "Orange");

            ViewModel.ReconfigureInvoked += OnReconfigureInvoked;
        }

        private void OnReconfigureInvoked(object? sender, EventArgs e)
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        private void InitializeChat()
        {
            ChatViewModel.SendMessageInvoked += OnSendMessage;
            ChatViewModel.SendEmojiInvoked += OnSendEmoji;
            ChatViewModel.PreventIdleInvoked += OnPreventIdle;
            ChatViewModel.ToggleKeyBindingsInvoked += OnToggleKeyBindings;
        }

        private void InitializeHub()
        {
            var baseUri = ConfigurationStore.ServerAddress;

            if (string.IsNullOrWhiteSpace(baseUri) || !baseUri.StartsWith("https://"))
                return;
            
            baseUri = baseUri.TrimEnd('/');

            connection = new HubConnectionBuilder()
                .WithUrl($"{baseUri}/PlayersHub").WithAutomaticReconnect()
                .Build();

            connection.Closed += async (error) =>
            {
                ConnectionViewModel.IsConnected = false;

                await Task.Delay(new Random().Next(0,5) * 1000);
                await connection.StartAsync();
            };

            connection.ServerTimeout = TimeSpan.FromHours(1.5);

            Hub = connection.CreateHubProxy<IPlayerHub>();
            var subscription = connection.Register<IPlayerClient>(this);
            
            ViewModel.Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        private void OnPreventIdle(object? sender, EventArgs e)
        {
            //if (FlyleafMe.Player.Activity.)

            FlyleafMe.Player.Activity.RefreshActive();
        }

        private void ReleaseSyncLock()
        {
            Task.Run(() =>
            {
                Thread.Sleep(200);
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
                if (Math.Abs(_last - _lastCurTime) >= 40000 * 1000)
                {
                    var direction = _last < _lastCurTime ? "⏩" : "⏪";

                    ChatViewModel.Messages.Add(new ChatMessage($"{direction} to {_last.TickToMovieString()}", false, true));

                    CleanupMessages();

                    await onSeeked();
                }
            }
            else if (e.PropertyName == "Status")
            {
                await onPlaybackStatusChanged();
            }
        }

        private async Task Connect()
        {
            try
            {
                await connection.StartAsync();

                await Hub.JoinGroup(ConfigurationStore.GroupName, ConfigurationStore.UserName);
                
                ViewModel.Player.Open(ConfigurationStore.FilePath);
                ViewModel.Player.Pause();

                ConnectionViewModel.IsConnected = true;

                await Dispatcher.InvokeAsync(() =>
                {
                    var storyboard = Resources["ConnectedStoryboard"] as Storyboard;
                    storyboard.Begin();
                });
            }
            catch (Exception ex)
            {
                ConnectionViewModel.IsConnected = false;
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
                var toggleTask = Hub.TogglePlayback(ConfigurationStore.GroupName, ConfigurationStore.UserName, Status, position);

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
                ConnectionViewModel.IsConnected = false;
            }
        }

        private async Task onSeeked()
        {
            try
            {
                SyncLock = true;

                var position = ViewModel.Player.CurTime;
                var toggleTask = Hub.TogglePlayback(ConfigurationStore.GroupName, ConfigurationStore.UserName, Status, position);

                await toggleTask;
            }
            catch (Exception e)
            {
                ConnectionViewModel.IsConnected = false;
            }
            finally
            {
                ReleaseSyncLock();
            }
        }

        private async void OnSendMessage(object? sender, EventArgs e)
        {
            if (!ChatViewModel.CanSendMessage)
                return;

            var message = ChatViewModel.MessageInput.Trim();

            try
            {
                ChatViewModel.MessageInput = "";

                var sendTask = Hub.SendMessage(ConfigurationStore.GroupName, ConfigurationStore.UserName, message);
                var addTask = Dispatcher.InvokeAsync(() =>
                {
                    ChatViewModel.Messages.Add(new ChatMessage($"{message}", true));

                    CleanupMessages();
                });
                
                await addTask;
                await sendTask;
            }
            catch (Exception ex)
            {
                
            }
        }
        
        private async void OnSendEmoji(object? sender, string emoji)
        {
            var message = EmojiExtensions.GetEmojiChar(emoji);
            
            try
            {
                var sendTask = Hub.SendMessage(ConfigurationStore.GroupName, ConfigurationStore.UserName, message);
                var addTask = Dispatcher.InvokeAsync(() =>
                {
                    ChatViewModel.Messages.Add(new ChatMessage($"{message}", true));

                    CleanupMessages();
                });

                await addTask;
                await sendTask;
            }
            catch (Exception ex)
            {
                
            }
        }

        private void OnToggleKeyBindings(object? sender, bool isEnabled)
        {
            FlyleafMe.KeyBindings = isEnabled ? AvailableWindows.Surface : AvailableWindows.None;
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

            await Dispatcher.InvokeAsync(() =>
            {
                string action = isPlaying ? "▶️" : "⏸️";
                
                ChatViewModel.Messages.Add(new ChatMessage($"{action} at {position.TickToMovieString()}", false, true));

                if (user != ConfigurationStore.UserName)
                {
                    ViewModel.Player.CurTime = position;
                
                    CleanupMessages();

                    if (isPlaying)
                        ViewModel.Player.Play();
                    else
                        ViewModel.Player.Pause();
                }

                ReleaseSyncLock();
            });
        }

        public async Task ReceiveMessage(string from, string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var messageText = $"{message}";

                ChatViewModel.Messages.Add(new ChatMessage(from, messageText));
                ChatViewModel.Notifications.Enqueue($"{from}: {messageText}");
                
                CleanupMessages();
            });
        }

        public async Task MemberJoined(string user)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var messageText = $"{user} joined";

                ChatViewModel.Messages.Add(new ChatMessage(messageText, false, true));
                ChatViewModel.Notifications.Enqueue(messageText);
                
                CleanupMessages();
            });
        }

        #endregion

    }
}
