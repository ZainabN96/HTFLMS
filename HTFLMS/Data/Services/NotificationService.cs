using HTFLMS.Data.IServices;
using HTFLMS.Dtos.Notifications;
using HTFLMS.Hubs;
using HTFLMS.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HTFLMS.Data.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext context;
        private readonly IHubContext<NotificationHub> hubContext;

        public NotificationService(
            ApplicationDbContext context,
            IHubContext<NotificationHub> hubContext)
        {
            this.context = context;
            this.hubContext = hubContext;
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(
            int userId,
            int take = 10)
        {
            return await context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(take)
                .Select(x => new NotificationDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    IsRead = x.IsRead,
                    RedirectUrl = x.RedirectUrl,
                    CreatedAt = x.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await context.Notifications
                .CountAsync(x =>
                    x.UserId == userId &&
                    !x.IsRead);
        }

        public async Task CreateAsync(
            int userId,
            string title,
            string message,
            string type,
            string? redirectUrl = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RedirectUrl = redirectUrl,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            context.Notifications.Add(notification);

            await context.SaveChangesAsync();

            var dto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                RedirectUrl = notification.RedirectUrl,
                CreatedAt = notification.CreatedAt
            };

            await hubContext.Clients
                .User(userId.ToString())
                .SendAsync("ReceiveNotification", dto);
        }

        public async Task CreateBulkAsync(
            IEnumerable<int> userIds,
            string title,
            string message,
            string type,
            string? redirectUrl = null)
        {
            var distinctUserIds = userIds
                .Distinct()
                .ToList();

            if (distinctUserIds.Count == 0)
                return;

            var notifications = distinctUserIds
                .Select(userId => new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    RedirectUrl = redirectUrl,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                })
                .ToList();

            await context.Notifications.AddRangeAsync(notifications);

            await context.SaveChangesAsync();

            foreach (var notification in notifications)
            {
                var dto = new NotificationDto
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    Type = notification.Type,
                    IsRead = notification.IsRead,
                    RedirectUrl = notification.RedirectUrl,
                    CreatedAt = notification.CreatedAt
                };

                await hubContext.Clients
                    .User(notification.UserId.ToString())
                    .SendAsync("ReceiveNotification", dto);
            }
        }

        public async Task<bool> MarkAsReadAsync(
            int notificationId,
            int userId)
        {
            var notification = await context.Notifications
                .FirstOrDefaultAsync(x =>
                    x.Id == notificationId &&
                    x.UserId == userId);

            if (notification == null)
                return false;

            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await context.SaveChangesAsync();
            }

            return true;
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await context.Notifications
                .Where(x =>
                    x.UserId == userId &&
                    !x.IsRead)
                .ToListAsync();

            if (notifications.Count == 0)
                return;

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await context.SaveChangesAsync();
        }
    }
}