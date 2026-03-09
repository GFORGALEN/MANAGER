namespace ConstructionManagement.Entities
{
    public class User
    {
        public Guid UserId { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string Role { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
