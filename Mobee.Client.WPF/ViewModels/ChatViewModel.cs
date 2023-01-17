using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Mobee.Client.WPF.Data;

namespace Mobee.Client.WPF.ViewModels
{
    public class ChatViewModel : ObservableObject
    {
        public ObservableCollection<ChatMessage> Messages { get; set; } = new();
    }
}
