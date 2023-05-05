using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;

namespace Mobee.Server.Aspnet.Utilities
{
    public static class HubContextExtensions
    {
        public static string? GetUserName(this HubCallerContext context, string? defaultName)
        {
            var identity = context.User?.Identities.OfType<GenericIdentity>().FirstOrDefault();
            return identity != null ? identity.Name : defaultName;
        }

    }
}
