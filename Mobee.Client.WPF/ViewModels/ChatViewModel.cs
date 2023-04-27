using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using Mobee.Client.WPF.Data;

namespace Mobee.Client.WPF.ViewModels
{
    public partial class ChatViewModel : ObservableObject
    {
        public event EventHandler? SendMessageInvoked;
        public event EventHandler<string>? SendEmojiInvoked; 
        public event EventHandler? PreventIdleInvoked;
        public event EventHandler<bool>? ToggleKeyBindingsInvoked; 

        [ObservableProperty]
        [NotifyPropertyChangedFor("CanSendMessage")]
        private string messageInput = "";
        
        public bool CanSendMessage => !string.IsNullOrWhiteSpace(MessageInput);
        
        [RelayCommand]
        public void SendMessage()
        {
            SendMessageInvoked?.Invoke(this, EventArgs.Empty);
        }
        
        [RelayCommand]
        public void SendEmoji(string? emoji)
        {
            if (emoji == null || string.IsNullOrWhiteSpace(emoji))
                return;

            SendEmojiInvoked?.Invoke(this, emoji);
        }
        
        public void PrevevntIdle()
        {
            PreventIdleInvoked?.Invoke(this, EventArgs.Empty);
        }

        public void ToggleKeyBindings(bool isEnabled)
        {
            ToggleKeyBindingsInvoked?.Invoke(this, isEnabled);
        }

        public ObservableCollection<ChatMessage> Messages { get; set; } = new();
        public ISnackbarMessageQueue Notifications { get; set; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3.0));

    }
}
