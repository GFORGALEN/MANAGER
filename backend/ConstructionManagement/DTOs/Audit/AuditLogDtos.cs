using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Audit
{
    public class AuditLogListDto
    {
        public Guid AuditLogId { get; set; }

        public Guid? ActorUserId { get; set; }

        public string ActorName { get; set; } = string.Empty;

        public string EntityType { get; set; } = string.Empty;

        public Guid EntityId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? FieldName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public string? Summary { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public static class AuditLogDtoMappings
    {
        public static AuditLogListDto ToListDto(this AuditLog auditLog)
        {
            return new AuditLogListDto
            {
                AuditLogId = auditLog.AuditLogId,
                ActorUserId = auditLog.ActorUserId,
                ActorName = auditLog.ActorName,
                EntityType = auditLog.EntityType,
                EntityId = auditLog.EntityId,
                Action = auditLog.Action,
                FieldName = auditLog.FieldName,
                OldValue = auditLog.OldValue,
                NewValue = auditLog.NewValue,
                Summary = auditLog.Summary,
                CreatedAt = auditLog.CreatedAt
            };
        }
    }
}
