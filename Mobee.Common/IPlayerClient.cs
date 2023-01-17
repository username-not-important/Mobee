namespace Mobee.Common
{
    public interface IPlayerClient
    {
        Task PlaybackToggled(string user, bool isPlaying);

        Task ReceiveMessage(string from, string message);
    }
}