using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FlyleafLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mobee.Client.WPF.IoC;
using Mobee.Client.WPF.Stores;
using Mobee.Client.WPF.ViewModels;
using Mobee.Common.Hubs;
using Logger = Mobee.Client.WPF.Logs.Logger;

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
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddSingleton<ConfigureWindow>();
                    services.AddSingleton<ConfigurationStore>();
                    services.AddAbstractFactory<ConfigurationViewModel>();
                }).Build();
        }

        public T GetRequiredService<T>()
        {
            return AppHost.Services.GetRequiredService<T>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            initializeFFmpeg();

            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<WPF.ConfigureWindow>();
            mainWindow.Show();

            Logger.Instance.Log("Starting Up...", true);

            base.OnStartup(e);

        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();

            base.OnExit(e);
        }

        private static void initializeFFmpeg()
        {
            string bits = Environment.Is64BitProcess ? "x64" : "x86";

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
                //LogOutput         = ":console",
                //LogOutput         = @"C:\Flyleaf\Logs\flyleaf.log",                
#endif

                //PluginsPath       = @"C:\Flyleaf\Plugins",

                UIRefresh = false, // Required for Activity, BufferedDuration, Stats in combination with Config.Player.Stats = true
                UIRefreshInterval = 250, // How often (in ms) to notify the UI
                UICurTimePerSecond =
                    true, // Whether to notify UI for CurTime only when it's second changed or by UIRefreshInterval
            });
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
