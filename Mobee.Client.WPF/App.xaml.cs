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
using Mobee.Client.WPF.ViewModels;
using Logger = Mobee.Client.WPF.Logs.Logger;

namespace Mobee.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IHost? AppHost { get; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<MainWindow>();
                    services.AddAbstractFactory<MainWindowViewModel>();
                }).Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            initializeFFmpeg();

            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<WPF.MainWindow>();
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
    }
}
