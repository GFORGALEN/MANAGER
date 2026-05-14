namespace ConstructionManagement.Entities
{
    public class AuditLog
    {
        public Guid AuditLogId { get; set; }

        public Guid? ActorUserId { get; set; }

        public string ActorName { get; set; } = "System";

        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? FieldName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? Summary { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public User? ActorUser { get; set; }
    }
}
