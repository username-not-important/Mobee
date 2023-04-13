using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobee.Client.WPF.Data;

namespace Mobee.Client.WPF.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        [ObservableProperty]
        [NotifyPropertyChangedFor("CanSendMessage")]
        private string messageInput = "";

        public bool CanSendMessage => !string.IsNullOrWhiteSpace(MessageInput);

        public ObservableCollection<ChatMessage> Messages { get; set; } = new();
        
    }
}
