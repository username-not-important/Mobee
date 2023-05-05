using Mobee.Client.WPF.Utilities;

namespace Mobee.Client.WPF.Data;

public class ChatMessage
{
    public ChatMessage(string message, bool isSelf = false, bool isBroadcast = false) : this("", message, isSelf, isBroadcast)
    {
    }
    
    public ChatMessage(string? sender, string message, bool isSelf = false, bool isBroadcast = false)
    {
        Sender = sender;
        Message = message;
        IsSelf = isSelf;
        IsBroadcast = isBroadcast;
    }

    public bool IsBroadcast { get; set; }

    public bool IsSelf { get; set; }

    public string? Sender { get; set; }

    public string Message { get; set; }

    public string DisplayedMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Message))
                return "";

            if (!IsSelf)
                return Message;

            var colonIndex = Message.IndexOf(':');
            return colonIndex == -1 ? Message : Message.Substring(colonIndex + 1).Trim();
        }
    }

    public string PureMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Message))
                return "";

            var colonIndex = Message.IndexOf(':');
            if (colonIndex == -1)
                return Message;

            return Message.Substring(colonIndex + 1).Trim();
        }
    }
    
    public bool IsAllEmoji => PureMessage.IsAllEmoji();
}