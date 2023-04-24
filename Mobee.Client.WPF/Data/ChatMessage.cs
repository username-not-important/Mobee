using Mobee.Client.WPF.Utilities;

namespace Mobee.Client.WPF.Data;

public class ChatMessage
{
    public ChatMessage(string message, bool isSelf = false, bool isBroadcast = false)
    {
        Message = message;
        IsSelf = isSelf;
        IsBroadcast = isBroadcast;
    }

    public bool IsBroadcast { get; set; }

    public bool IsSelf { get; set; }

    public string Message { get; set; }

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

    public bool IsXLEmoji
    {
        get
        {
            var stringInfo = new System.Globalization.StringInfo(PureMessage);
            var length = stringInfo.LengthInTextElements;

            return !string.IsNullOrWhiteSpace(Message) && length == 1 && PureMessage.HasEmoji();
        }
    }
}