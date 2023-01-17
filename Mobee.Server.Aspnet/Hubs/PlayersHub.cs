using Microsoft.AspNetCore.SignalR;
using Mobee.Common;

namespace Mobee.Server.Aspnet.Hubs
{
    public class PlayersHub : Hub<IPlayerClient>, IPlayerHub
    {
        public async Task TogglePlayback(string user, bool isPlaying)
        {
            await Clients.All.PlaybackToggled(user, isPlaying);
        }

        public async Task SendMessage(string from, string message)
        {
            await Clients.Others.ReceiveMessage(from, message);
        }
    }
}
