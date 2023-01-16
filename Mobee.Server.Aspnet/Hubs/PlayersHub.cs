using Microsoft.AspNetCore.SignalR;

namespace Mobee.Server.Aspnet.Hubs
{
    public class PlayersHub : Hub
    {
        public async Task TogglePlayback(string user, bool isPlaying)
        {
            await Clients.All.SendAsync("PlaybackToggled", user, isPlaying);
        }
    }
}
