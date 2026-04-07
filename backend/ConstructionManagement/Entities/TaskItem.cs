namespace ConstructionManagement.Entities
{
    public class TaskItem
    {
        public Guid TaskItemId { get; set; }

        public Guid ProjectId { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public string Status { get; set; } = "Draft";

        public string Priority { get; set; } = "Medium";

        public string? Category { get; set; }

        public decimal? EstimatedHours { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public Guid? AssignedUserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;

        public User? AssignedUser { get; set; }

        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}
