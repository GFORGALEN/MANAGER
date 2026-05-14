namespace ConstructionManagement.Entities
{
    public class Notification
    {
        public Guid NotificationId { get; set; }

        public Guid RecipientUserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Category { get; set; } = "General";

        public string? EntityType { get; set; }

        public Guid? EntityId { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User RecipientUser { get; set; } = null!;
    }
}
