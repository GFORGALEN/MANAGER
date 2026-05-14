using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Notifications;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<NotificationListDto>> GetUserNotificationsAsync(Guid userId, NotificationQueryDto query, CancellationToken cancellationToken = default)
        {
            var notifications = _context.Notifications.Where(notification => notification.RecipientUserId == userId);

            if (query.IsRead.HasValue)
            {
                notifications = notifications.Where(notification => notification.IsRead == query.IsRead.Value);
            }

            var (items, totalCount) = await notifications
                .OrderByDescending(notification => notification.CreatedAt)
                .Select(notification => notification.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<NotificationListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<NotificationSummaryDto> GetUserNotificationSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var items = await _context.Notifications
                .Where(notification => notification.RecipientUserId == userId)
                .OrderByDescending(notification => notification.CreatedAt)
                .Take(8)
                .Select(notification => notification.ToListDto())
                .ToListAsync(cancellationToken);

            var unreadCount = await _context.Notifications
                .CountAsync(notification => notification.RecipientUserId == userId && !notification.IsRead, cancellationToken);

            return new NotificationSummaryDto
            {
                UnreadCount = unreadCount,
                Items = items
            };
        }

        public async Task<bool> MarkAsReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(item => item.NotificationId == notificationId && item.RecipientUserId == userId, cancellationToken);

            if (notification is null)
            {
                return false;
            }

            notification.IsRead = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _context.Notifications
                .Where(notification => notification.RecipientUserId == userId && !notification.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return notifications.Count;
        }

        public async Task CreateForUsersAsync(IEnumerable<Guid> userIds, string title, string message, string category, string? entityType, Guid? entityId, CancellationToken cancellationToken = default)
        {
            var distinctIds = userIds.Distinct().ToList();
            if (distinctIds.Count == 0)
            {
                return;
            }

            var activeIds = await _context.Users
                .Where(user => distinctIds.Contains(user.UserId) && user.IsActive)
                .Select(user => user.UserId)
                .ToListAsync(cancellationToken);

            _context.Notifications.AddRange(activeIds.Select(userId => new Notification
            {
                NotificationId = Guid.NewGuid(),
                RecipientUserId = userId,
                Title = title,
                Message = message,
                Category = category,
                EntityType = entityType,
                EntityId = entityId,
                CreatedAt = DateTime.UtcNow
            }));

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CreateForRolesAsync(IEnumerable<string> roles, string title, string message, string category, string? entityType, Guid? entityId, CancellationToken cancellationToken = default)
        {
            var normalizedRoles = roles.Select(role => role.Trim()).Where(role => role.Length > 0).Distinct().ToList();
            if (normalizedRoles.Count == 0)
            {
                return;
            }

            var userIds = await _context.Users
                .Where(user => user.IsActive && normalizedRoles.Contains(user.Role))
                .Select(user => user.UserId)
                .ToListAsync(cancellationToken);

            await CreateForUsersAsync(userIds, title, message, category, entityType, entityId, cancellationToken);
        }
    }
}
