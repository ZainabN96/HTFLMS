using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HTFLMS.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
    }
}