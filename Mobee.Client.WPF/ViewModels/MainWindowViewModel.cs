using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlyleafLib.MediaPlayer;
using FlyleafLib;

namespace Mobee.Client.WPF.ViewModels
{
    public class MainWindowViewModel
    {
        public ChatViewModel ChatViewModel { get; set; } = new();
        
        public Player Player { get; set; }

        public Config Config { get; set; }

        public MainWindowViewModel()
        {
            Config = new Config();
            Config.Player.SeekAccurate = true;
            Config.Player.AutoPlay = false;

            Player = new Player(Config);
        }
    }
}
