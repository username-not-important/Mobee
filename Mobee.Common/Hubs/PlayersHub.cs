using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;
using Mobee.Common.Utilities;

namespace Mobee.Common.Hubs
{
    public class PlayersHub : Hub<IPlayerClient>, IPlayerHub
    {
        public async Task JoinGroup(string group, string user)
        {
            Context.User.AddIdentity(new GenericIdentity(user));
            
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.OthersInGroup(group).MemberJoined(user);
        }

        public async Task TogglePlayback(string group, string user, bool isPlaying, long position)
        {
            await Clients.Group(group).PlaybackToggled(Context.getUserName(user), isPlaying, position);
        }

        public async Task SendMessage(string group, string user, string message)
        {
            await Clients.OthersInGroup(group).ReceiveMessage(Context.getUserName(user), message);
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
