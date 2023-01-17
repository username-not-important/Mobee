using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Client.WPF.ViewModels
{
    public class MainWindowViewModel
    {
        public ChatViewModel ChatViewModel { get; set; } = new();
    }
}
