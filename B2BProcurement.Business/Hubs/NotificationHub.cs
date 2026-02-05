using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace B2BProcurement.Business.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time notifications.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            var companyId = Context.User?.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(companyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"company_{companyId}");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            var companyId = Context.User?.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(companyId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"company_{companyId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
