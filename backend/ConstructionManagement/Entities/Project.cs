namespace ConstructionManagement.Entities
{
    public class Project
    {
        public Guid ProjectId { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public required string Address { get; set; }

        public string? Description { get; set; }

        public string? ClientName { get; set; }

        public string Status { get; set; } = "Planning";

        public decimal? Budget { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();

        public ICollection<Variation> Variations { get; set; } = new List<Variation>();

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
