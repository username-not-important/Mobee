using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Common
{
    public interface IPlayerHub
    {
        Task JoinGroup(string group, string user);

        Task TogglePlayback(string group, string user, bool isPlaying, long position);

        Task SendMessage(string group, string user, string message);
    }
}
