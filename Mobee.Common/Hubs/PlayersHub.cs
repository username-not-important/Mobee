﻿using Microsoft.AspNetCore.SignalR;

namespace Mobee.Common.Hubs
{
    public class PlayersHub : Hub<IPlayerClient>, IPlayerHub
    {
        public async Task TogglePlayback(string user, bool isPlaying, long position)
        {
            await Clients.Others.PlaybackToggled(user, isPlaying, position);
        }

        public async Task SendMessage(string from, string message)
        {
            await Clients.Others.ReceiveMessage(from, message);
        }
    }
}