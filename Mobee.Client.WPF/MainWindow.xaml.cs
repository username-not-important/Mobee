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
using System.Timers;
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
using System.Windows.Threading;
using FlyleafLib.MediaPlayer;
using FlyleafLib;
using FlyleafLib.Controls.WPF;
using Microsoft.AspNetCore.SignalR.Client;
using Mobee.Client.WPF.Connection;
using Mobee.Client.WPF.Data;
using Mobee.Client.WPF.IoC;
using Mobee.Client.WPF.Stores;
using Mobee.Client.WPF.ViewModels;
using Mobee.Common;
using TypedSignalR.Client;
using Mobee.Client.WPF.Utilities;
using Mobee.Client.WPF.Utilities.Validators;
using Polly;
using Polly.Retry;
using IRetryPolicy = Microsoft.AspNetCore.SignalR.Client.IRetryPolicy;
using Logger = Mobee.Client.WPF.Logs.Logger;
using Timer = System.Timers.Timer;

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

        #region Initialization

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            var connectionResult = await Connect();
            if (!connectionResult)
            {
                MessageBox.Show("Failed to Connect to Server... Please Reconfigure the Application.");

                App.Restart();
            }
            else
            {
                await onConnect();
            }

            await InitializePlayer();
        }
        
        private async Task InitializePlayer()
        {
            var uiResize = Dispatcher.InvokeAsync(() =>
            {
                var ratio = ViewModel.Player.Video.AspectRatio;
                Height = ActualWidth / ratio.Value + 30;

                FlyleafMe.SelectedTheme = FlyleafMe.UIConfig.Themes.FirstOrDefault(x => x.Name == "Orange");
            });
            
            var uiReady = Dispatcher.InvokeAsync(() =>
            {
                var storyboard = Resources["ConnectedStoryboard"] as Storyboard;
                storyboard!.Begin();
            });

            ViewModel.ReconfigureInvoked += (sender, args) => App.Restart();
            ViewModel.ChangeLanguageInvoked += (sender, args) =>
            {
                Properties.Settings.Default.CULTURE = null;
                Properties.Settings.Default.Save();

                App.Restart();
            };

            await uiResize;
            await uiReady;
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
            var baseUrl = ConfigurationStore.ServerAddress;

            Logger.Instance.Log($"Initializing Hub: {baseUrl}", Logger.CH_COMMS);

            if (!UrlValidator.IsHubUrlValid(baseUrl))
            {
                MessageBox.Show("Selected Server is Invalid!");
                return;
            }

            baseUrl = baseUrl.TrimEnd('/');

            BuildConnection(baseUrl);

            ViewModel.Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        #endregion

        #region Connection
        
        private void BuildConnection(string baseUrl)
        {
            connection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/PlayersHub").WithAutomaticReconnect(new MobeeReconnectPolicy())
                .Build();

            connection.ServerTimeout = TimeSpan.FromHours(1.5);
            connection.Closed += OnConnectionClosed;
            connection.Reconnected += OnReconnected;
            connection.Reconnecting += OnReconnecting;

            Hub = connection.CreateHubProxy<IPlayerHub>();
            var subscription = connection.Register<IPlayerClient>(this);
        }

        private async Task OnConnectionClosed(Exception? ex)
        {
            Logger.Instance.Log($"Hub Connection Closed\r\n{ex}", Logger.CH_COMMS);

            ConnectionViewModel.IsConnected = false;

            await Task.Delay(new Random().Next(1, 5) * 1000);
            await connection.StartAsync();
        }

        private async Task OnReconnected(string? ex)
        {
            Logger.Instance.Log($"Hub Reconnected\r\n{ex}", Logger.CH_COMMS);
            
            ConnectionViewModel.IsConnected = true;

            try
            {
                await Hub.JoinGroup(ConfigurationStore.GroupName, ConfigurationStore.UserName);
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"Join Group Failed\r\n{e}", Logger.CH_COMMS);
            }
        }
        
        private Task OnReconnecting(Exception? ex)
        {
            Logger.Instance.Log($"Hub Reconnection Initiated\r\n{ex}", Logger.CH_COMMS);

            ConnectionViewModel.IsConnected = false;

            return Task.CompletedTask;
        }
        
        private async Task<bool> Connect()
        {
            try
            {
                await connection.StartAsync();
                await Hub.JoinGroup(ConfigurationStore.GroupName, ConfigurationStore.UserName);
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Connect to Hub Failed\r\n{ex}", Logger.CH_COMMS);

                return false;
            }
        }

        private Task onConnect()
        {
            ViewModel.Player.Open(ConfigurationStore.FilePath);
            ViewModel.Player.CurTime = 0;
            ViewModel.Player.Pause();

            ConnectionViewModel.IsConnected = true;

            Timer queryTimer = new Timer(TimeSpan.FromSeconds(5));
            queryTimer.Elapsed += QueryTimerOnElapsed;
            queryTimer.Start();

            return Task.CompletedTask;
        }

        #endregion
        
        #region PlayerHandlers

        private void OnPreventIdle(object? sender, EventArgs e)
        {
            FlyleafMe.Player.Activity.RefreshActive();
        }

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
                    ChatViewModel.AddMessage(ChatMessage.FromSeek(_last, _lastCurTime));

                    await OnSeeked();
                }
            }
            else if (e.PropertyName == "Status")
            {
                await OnPlaybackStatusChanged();
            }
        }

        private async Task OnPlaybackStatusChanged()
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
                Logger.Instance.Log($"Syncing PlaybackStatus Failed:\r\n{ex}", Logger.CH_COMMS);

                ConnectionViewModel.IsConnected = false;
            }
        }

        private async Task OnSeeked()
        {
            try
            {
                SyncLock = true;

                var position = ViewModel.Player.CurTime;
                var toggleTask = Hub.TogglePlayback(ConfigurationStore.GroupName, ConfigurationStore.UserName, Status, position);

                await toggleTask;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Syncing Seek Failed:\r\n{ex}", Logger.CH_COMMS);

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
                    ChatViewModel.AddMessage(new ChatMessage($"{message}", true));
                });

                await addTask;
                await sendTask;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Send Message Failed:\r\n{ex}", Logger.CH_COMMS);

                ConnectionViewModel.IsConnected = false;
            }
        }

        private async void OnSendEmoji(object? sender, string emoji)
        {
            var message = emoji;

            try
            {
                var sendTask = Hub.SendMessage(ConfigurationStore.GroupName, ConfigurationStore.UserName, message);
                var addTask = Dispatcher.InvokeAsync(() =>
                {
                    ChatViewModel.AddMessage(new ChatMessage($"{message}", true));
                });

                await addTask;
                await sendTask;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Send Emoji Failed:\r\n{ex}", Logger.CH_COMMS);

                ConnectionViewModel.IsConnected = false;
            }
        }

        private void OnToggleKeyBindings(object? sender, bool isEnabled)
        {
            FlyleafMe.KeyBindings = isEnabled ? AvailableWindows.Surface : AvailableWindows.None;
        }

        #endregion
        
        private void ReleaseSyncLock()
        {
            Task.Run(() =>
            {
                Thread.Sleep(200);
                SyncLock = false;
            });
        }

        private long _lastCurTime = 0;

        private bool _queryInProgress = false;
        private async void QueryTimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            try
            {
                if (_queryInProgress)
                    return;

                _queryInProgress = true;

                var users = await Hub.QueryGroupUsers(ConfigurationStore.GroupName!, ConfigurationStore.UserName!);

                await Dispatcher.InvokeAsync(() =>
                {
                    ChatViewModel.UpdateOnlineUsers(users);
                });
            }
            catch (Exception ex)
            {
                Logger.Instance.Log($"Query Group Users Failed:\r\n{ex}", Logger.CH_COMMS);
            }
            finally
            {
                _queryInProgress = false;
            }
        }


        #region IPlaybackClient (Receiving)

        public async Task PlaybackToggled(string? user, bool isPlaying, long position)
        {
            if (SyncLock)
                return;

            if (isPlaying && position < 0)
                return;

            SyncLock = true;

            Status = isPlaying;

            await Dispatcher.InvokeAsync(() =>
            {
                ChatViewModel.AddMessage(ChatMessage.FromPlaybackToggle(isPlaying, position));

                if (user != ConfigurationStore.UserName)
                {
                    ViewModel.Player.CurTime = position;

                    if (isPlaying)
                        ViewModel.Player.Play();
                    else
                        ViewModel.Player.Pause();
                }

                ReleaseSyncLock();
            });
        }

        public async Task ReceiveMessage(string? from, string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var messageText = $"{message}";

                ChatViewModel.AddMessage(new ChatMessage(from, messageText));
                ChatViewModel.Notifications.Enqueue($"{from}: {messageText}");
            });
        }

        public async Task MemberJoined(string? user)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var message = ChatMessage.FromMemberJoined(user!);

                ChatViewModel.AddMessage(message);
                ChatViewModel.Notifications.Enqueue(message.Message);
            });
        }

        public async Task MemberLeft(string user)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var message = ChatMessage.FromMemberLeft(user);

                ChatViewModel.AddMessage(message);
                ChatViewModel.Notifications.Enqueue(message.Message);
            });
        }

        #endregion

    }
}
