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
        public ConnectionViewModel ConnectionViewModel { get; set; } = new();
        
        public Player Player { get; set; }

        public Config Config { get; set; }

        public MainWindowViewModel()
        {
            Config = new Config
            {
                Player =
                {
                    SeekAccurate = true,
                    AutoPlay = false,
                    
                }
            };

            Player = new Player(Config);
        }
    }
}
