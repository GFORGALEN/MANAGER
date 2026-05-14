using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Notifications;

namespace ConstructionManagement.Services
{
    public interface INotificationService
    {
        Task<PagedResultDto<NotificationListDto>> GetUserNotificationsAsync(Guid userId, NotificationQueryDto query, CancellationToken cancellationToken = default);

        Task<NotificationSummaryDto> GetUserNotificationSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

        Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default);

        Task CreateForUsersAsync(IEnumerable<Guid> userIds, string title, string message, string category, string? entityType, Guid? entityId, CancellationToken cancellationToken = default);

        Task CreateForRolesAsync(IEnumerable<string> roles, string title, string message, string category, string? entityType, Guid? entityId, CancellationToken cancellationToken = default);
    }
}
