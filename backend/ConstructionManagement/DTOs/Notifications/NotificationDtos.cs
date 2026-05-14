using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Notifications
{
    public class NotificationListDto
    {
        public Guid NotificationId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public class NotificationSummaryDto
    {
        public int UnreadCount { get; set; }

        public List<NotificationListDto> Items { get; set; } = [];
    }

    public static class NotificationDtoMappings
    {
        public static NotificationListDto ToListDto(this Notification notification)
        {
            return new NotificationListDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Message = notification.Message,
                Category = notification.Category,
                EntityType = notification.EntityType,
                EntityId = notification.EntityId,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
