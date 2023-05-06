using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mobee.Client.WPF.Stores
{
    /// <summary>
    /// A Class to Store variables before Transfering between Different Window Instances
    /// </summary>
    public class ConfigurationStore : StoreBase
    {
        public string FilePath { get; set; } = "";

        public string ServerAddress { get; set; } = "";

        public string? UserName { get; set; } = "";

        public string? GroupName { get; set; } = "";
    }
}
