namespace ConstructionManagement.Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        public required string Name { get; set; }

        public required string Username { get; set; }

        public required string Email { get; set; }

        public string? PhoneNumber { get; set; }

        public required string PasswordHash { get; set; }

        public required string Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    }
}
