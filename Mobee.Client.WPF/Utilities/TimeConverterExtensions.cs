using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Client.WPF.Utilities
{
    public static class TimeConverterExtensions
    {
        public static string TickToMovieString(this long ticks)
        {
            return TimeSpan.FromMilliseconds(ticks / 10000.0).ToString("hh\\:mm\\:ss");
        }
    }
}
