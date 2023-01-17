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
}