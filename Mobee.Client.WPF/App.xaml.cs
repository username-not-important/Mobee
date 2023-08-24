using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FlyleafLib;
using FlyleafLib.Controls.WPF;
using Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mobee.Client.WPF.Assets;
using Mobee.Client.WPF.IoC;
using Mobee.Client.WPF.Stores;
using Mobee.Client.WPF.ViewModels;
using Mobee.Common.Hubs;
using Logger = Mobee.Client.WPF.Logs.Logger;
using Settings = Mobee.Client.WPF.Properties.Settings;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string __API_Version => HubMeta.__API_Version;

        public IHost? AppHost { get; }

        public App()
        {
            LocalizationManager.Init(new DefaultVocabolaryServiceProvider { }, new CultureInfo(Settings.Default.CULTURE ?? "en-US"));
            
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<LanguageWindow>();
                    services.AddSingleton<ConfigureWindow>();
                    services.AddSingleton<ConfigurationStore>();
                    services.AddAbstractFactory<ConfigurationViewModel>();
                }).Build();

            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.Instance.Log($"Unhandled Exception:\r\n{e.Exception}", Logger.CH_APP);
        }

        public T GetRequiredService<T>()
        {
            return AppHost.Services.GetRequiredService<T>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (!initializeFFmpeg())
            {
                MessageBox.Show("Mobee Startup Failed... Reason: FFMPEG");
                App.Current.Shutdown(101);
            };

            await AppHost!.StartAsync();

            if (string.IsNullOrWhiteSpace(Settings.Default.CULTURE))
            {
                var window = AppHost.Services.GetRequiredService<LanguageWindow>();
                window.Show();
            }
            else
            {
                StartConfiguration();
            }
            
            Logger.Instance.Log("Startup Complete...", true);

            base.OnStartup(e);
        }

        public void StartConfiguration()
        {
            var window = AppHost.Services.GetRequiredService<ConfigureWindow>();
            window.Show();
        }
        
        public static void Restart()
        {
            System.Windows.Forms.Application.Restart();
            System.Windows.Application.Current.Shutdown();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();

            base.OnExit(e);
        }

        private static bool initializeFFmpeg()
        {
            try
            {
                string bits = Environment.Is64BitProcess ? "x64" : "x86";

                Logger.Instance.Log($"Initializing FFMPEG:{bits}", Logger.CH_PLAYER);

                Engine.Start(new EngineConfig()
                {
                    FFmpegPath = Environment.CurrentDirectory + $"\\Libs\\{bits}\\FFmpeg\\",
                    FFmpegDevices =
                        false, // Prevents loading avdevice/avfilter dll files. Enable it only if you plan to use dshow/gdigrab etc.
                    HighPerformaceTimers = false, // Forces TimeBeginPeriod(1) always active (Use this for multiple players)

#if RELEASE
                FFmpegLogLevel = FFmpegLogLevel.Quiet,
                LogLevel = LogLevel.Quiet,
#else
                    FFmpegLogLevel = FFmpegLogLevel.Warning,
                    LogLevel = LogLevel.Debug,
                    LogOutput = ":debug",   
#endif
                
                    UIRefresh = false, // Required for Activity, BufferedDuration, Stats in combination with Config.Player.Stats = true
                    UIRefreshInterval = 250, // How often (in ms) to notify the UI
                    UICurTimePerSecond = true, // Whether to notify UI for CurTime only when it's second changed or by UIRefreshInterval
                });

                return true;
            }
            catch (Exception e)
            {
                Logger.Instance.Log($"FFMPEG Engine Start Failed\r\n{e}", Logger.CH_PLAYER);

                return false;
            }
        }
        
        public static T VMLocator<T>()
        {
            string type = typeof(T).Name;

            return (T)Current.Resources[type];
        }

        public static T FindResource<T>(string v)
        {
            return (T)Current.Resources[v];
        }

    }
}
