using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Mobee.Common.Utilities
{
    internal static class HubContextExtensions
    {
        public static string getUserName(this HubCallerContext context, string defaultName)
        {
            var identity = context.User.Identities.OfType<GenericIdentity>().FirstOrDefault();
            return identity != null ? identity.Name : defaultName;
        }

    }
}
