using Microsoft.AspNetCore.SignalR;

namespace Mobee.Common.Hubs
{
    public class PlayersHub : Hub<IPlayerClient>, IPlayerHub
    {
        public async Task JoinGroup(string group)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
        }

        public async Task TogglePlayback(string group, string user, bool isPlaying, long position)
        {
            await Clients.OthersInGroup(group).PlaybackToggled(user, isPlaying, position);
        }

        public async Task SendMessage(string group, string user, string message)
        {
            await Clients.OthersInGroup(group).ReceiveMessage(user, message);
        }
        
        #region Overrides

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
