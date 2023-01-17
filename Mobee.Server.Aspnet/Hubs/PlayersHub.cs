using Microsoft.AspNetCore.SignalR;

namespace Mobee.Server.Aspnet.Hubs
{
    public class PlayersHub : Hub
    {
        public async Task TogglePlayback(string user, bool isPlaying)
        {
            await Clients.All.SendAsync("PlaybackToggled", user, isPlaying);
        }

        public async Task SendMessage(string from, string message)
        {
            await Clients.Others.SendAsync("ReceiveMessage", from, message);
        }
    }
}
