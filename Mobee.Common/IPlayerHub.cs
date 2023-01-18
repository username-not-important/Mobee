using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Common
{
    public interface IPlayerHub
    {
        Task TogglePlayback(string user, bool isPlaying, long position);

        Task SendMessage(string from, string message);
    }
}
