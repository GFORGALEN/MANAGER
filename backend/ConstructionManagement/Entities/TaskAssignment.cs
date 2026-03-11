namespace ConstructionManagement.Entities
{
    public class TaskAssignment
    {
        public Guid TaskItemId { get; set; }

        public Guid UserId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public TaskItem TaskItem { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
