using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public ChatViewModel()
        {
            EmojiList = new List<string>()
            {
                "😂", "😍", "😘", "🥺", "😭", "☹️", "😐", "💖", "😡"
            };

            OnlineUsers.CollectionChanged += (s,e) => OnPropertyChanged(nameof(OnlineUsersNames));
        }
        
        [ObservableProperty]
        [NotifyPropertyChangedFor("CanSendMessage")]
        private string messageInput = "";

        private void TidyUp()
        {
            if (Messages.Count <= 10) 
                return;

            var removeList = Messages.Take(5).Where(chatMessage => chatMessage.IsBroadcast).ToList();

            foreach (var chatMessage in removeList)
            {
                Messages.Remove(chatMessage);
            }
        }

        #region Events

        public event EventHandler? SendMessageInvoked;
        public event EventHandler<string>? SendEmojiInvoked; 
        public event EventHandler? PreventIdleInvoked;
        public event EventHandler<bool>? ToggleKeyBindingsInvoked; 

        #endregion
        
        #region Commands

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

        #endregion

        #region Properties

        public List<string> EmojiList { get; }

        public string OnlineUsersNames => OnlineUsers.Count == 0 ? "no one else is online..." : string.Join(", ", OnlineUsers);

        public ObservableCollection<string> OnlineUsers { get; } = new();
        public ObservableCollection<ChatMessage> Messages { get; } = new();
        public ISnackbarMessageQueue Notifications { get; set; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3.0));

        #endregion

        public void UpdateOnlineUsers(List<string> users)
        {
            OnlineUsers.Clear();
            foreach (var user in users)
            {
                OnlineUsers.Add(user);
            }
        }

        public void AddMessage(ChatMessage chatMessage)
        {
            Messages.Add(chatMessage);

            TidyUp();
        }
    }
}
