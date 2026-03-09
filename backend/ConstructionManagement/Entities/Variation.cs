namespace ConstructionManagement.Entities
{
    public class Variation
    {
        public Guid VariationId { get; set; }

        public Guid ProjectId { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public string Status { get; set; } = "Draft";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;
    }
}
