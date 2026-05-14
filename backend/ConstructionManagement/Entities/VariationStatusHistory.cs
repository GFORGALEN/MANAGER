namespace ConstructionManagement.Entities
{
    public class VariationStatusHistory
    {
        public Guid VariationStatusHistoryId { get; set; }

        public Guid VariationId { get; set; }

        public string FromStatus { get; set; } = string.Empty;

        public string ToStatus { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public Guid? ActorUserId { get; set; }

        public string ActorName { get; set; } = "System";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Variation Variation { get; set; } = null!;

        public User? ActorUser { get; set; }
    }
}
