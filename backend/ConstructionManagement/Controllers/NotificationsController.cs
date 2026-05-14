using System.Security.Claims;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Notifications;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet("api/notifications")]
        public async Task<ActionResult<PagedResultDto<NotificationListDto>>> GetNotifications([FromQuery] NotificationQueryDto query, CancellationToken cancellationToken)
        {
            return Ok(await _notificationService.GetUserNotificationsAsync(GetCurrentUserId(), query, cancellationToken));
        }

        [HttpGet("api/notifications/summary")]
        public async Task<ActionResult<NotificationSummaryDto>> GetSummary(CancellationToken cancellationToken)
        {
            return Ok(await _notificationService.GetUserNotificationSummaryAsync(GetCurrentUserId(), cancellationToken));
        }

        [HttpPatch("api/notifications/{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken cancellationToken)
        {
            if (!await _notificationService.MarkAsReadAsync(GetCurrentUserId(), id, cancellationToken))
            {
                return NotFound();
            }

            return Ok();
        }

        [HttpPost("api/notifications/read-all")]
        public async Task<ActionResult<object>> MarkAllAsRead(CancellationToken cancellationToken)
        {
            var count = await _notificationService.MarkAllAsReadAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new { count });
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Missing user id claim.");
            return Guid.Parse(userIdValue);
        }
    }
}
