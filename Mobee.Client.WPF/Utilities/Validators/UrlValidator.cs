using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mobee.Client.WPF.Logs;

namespace Mobee.Client.WPF.Utilities.Validators
{
    public class UrlValidator
    {
        public static bool IsHubUrlValid(string url)
        {
            bool result = !string.IsNullOrWhiteSpace(url) && url.StartsWith("https://");

            if (!result)
                Logger.Instance.Log($"Invalid Hub Uri: {url}", Logger.CH_COMMS);

            return result;
        }
    }
}
