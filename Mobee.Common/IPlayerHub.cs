using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Common
{
    public interface IPlayerHub
    {
        //public const string __API_Version = "0.3.0-alpha";

        Task JoinGroup(string group, string user);

        Task TogglePlayback(string group, string user, bool isPlaying, long position);

        Task SendMessage(string group, string user, string message);
    }
}
