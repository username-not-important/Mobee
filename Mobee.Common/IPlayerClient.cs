namespace Mobee.Common
{
    public interface IPlayerClient
    {
        Task PlaybackToggled(string? user, bool isPlaying, long position);

        Task ReceiveMessage(string? from, string message);

        Task MemberJoined(string? user);

        Task MemberLeft(string user);
    }
}