using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;
using Mobee.Common;
using Mobee.Server.Aspnet.Utilities;

namespace Mobee.Server.Aspnet.Hubs
{
    public class PlayersHub : Hub<IPlayerClient>, IPlayerHub
    {
        public UsersRepository Users { get; }

        public PlayersHub(UsersRepository users)
        {
            Users = users;
        }

        public async Task JoinGroup(string? group, string? user)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(group))
                return;

            Context.User?.AddIdentity(new GenericIdentity(user));

            Users.UserConnected(Context.ConnectionId, user, group);

            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.OthersInGroup(group).MemberJoined(user);
        }

        public async Task<List<string>> QueryGroupUsers(string group, string user)
        {
            return Users.GetGroupUsersExcept(group, user);
        }

        public async Task TogglePlayback(string? group, string? user, bool isPlaying, long position)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(group))
                return;

            await Clients.Group(group).PlaybackToggled(Context.GetUserName(user), isPlaying, position);
        }

        public async Task SendMessage(string? group, string? user, string message)
        {
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(group))
                return;

            await Clients.OthersInGroup(group).ReceiveMessage(Context.GetUserName(user), message);
        }

        #region Overrides

        public override async Task OnConnectedAsync()
        {
            var userName = Context.GetUserName(null);

            Users.UserConnected(Context.ConnectionId, userName, null);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userName = Context.GetUserName(null);
            if (userName != null)
            {
                var group = Users.GetUserGroup(Context.ConnectionId);
                if (group != null)
                    await Clients.Group(group).MemberLeft(userName);
            }

            Users.UserDisconnected(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        #endregion
    }
}
