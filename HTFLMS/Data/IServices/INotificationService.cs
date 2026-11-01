using HTFLMS.Dtos.Notifications;

namespace HTFLMS.Data.IServices
{
    public interface INotificationService
    {
        Task<List<NotificationDto>> GetUserNotificationsAsync(
            int userId,
            int take = 10);

        Task<int> GetUnreadCountAsync(int userId);

        Task CreateAsync(
            int userId,
            string title,
            string message,
            string type,
            string? redirectUrl = null);

        Task CreateBulkAsync(
            IEnumerable<int> userIds,
            string title,
            string message,
            string type,
            string? redirectUrl = null);

        Task<bool> MarkAsReadAsync(
            int notificationId,
            int userId);

        Task MarkAllAsReadAsync(int userId);
    }
}