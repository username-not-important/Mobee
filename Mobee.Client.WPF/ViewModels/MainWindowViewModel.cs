using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FlyleafLib.MediaPlayer;
using FlyleafLib;

namespace Mobee.Client.WPF.ViewModels
{
    public partial class MainWindowViewModel
    {
        public event EventHandler? ReconfigureInvoked;

        public Player Player { get; set; }

        public Config Config { get; set; }

        [RelayCommand]
        public void InvokeReconfigure()
        {
            ReconfigureInvoked?.Invoke(this, EventArgs.Empty);
        }

        public MainWindowViewModel()
        {
            Config = new Config
            {
                Player =
                {
                    SeekAccurate = true,
                    AutoPlay = false
                }
            };

            Player = new Player(Config);
        }
    }
}
