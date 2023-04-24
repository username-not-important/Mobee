using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;

namespace Mobee.Server.Aspnet.Utilities
{
    public static class HubContextExtensions
    {
        public static string GetUserName(this HubCallerContext context, string defaultName)
        {
            var identity = Enumerable.OfType<GenericIdentity>(context.User.Identities).FirstOrDefault();
            return identity != null ? identity.Name : defaultName;
        }

    }
}
