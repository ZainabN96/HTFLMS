using System.Security.Claims;
using HTFLMS.Data.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTFLMS.Controllers.api
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly IUnitOfWork uow;

        public NotificationController(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized();

            var notifications =
                await uow.NotificationService
                    .GetUserNotificationsAsync(userId);

            return Ok(new
            {
                success = true,
                data = notifications
            });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized();

            var count =
                await uow.NotificationService
                    .GetUnreadCountAsync(userId);

            return Ok(new
            {
                success = true,
                count
            });
        }

        [HttpPost("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized();

            var result =
                await uow.NotificationService
                    .MarkAsReadAsync(id, userId);

            if (!result)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Notification not found."
                });
            }

            return Ok(new
            {
                success = true
            });
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();

            if (userId <= 0)
                return Unauthorized();

            await uow.NotificationService
                .MarkAllAsReadAsync(userId);

            return Ok(new
            {
                success = true
            });
        }

        private int GetCurrentUserId()
        {
            var claimValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("UserId");

            return int.TryParse(
                claimValue,
                out var userId)
                ? userId
                : 0;
        }
    }
}