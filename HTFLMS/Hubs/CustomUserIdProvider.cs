using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace HTFLMS.Hubs
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            return connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? connection.User?.FindFirstValue("UserId");
        }
    }
}