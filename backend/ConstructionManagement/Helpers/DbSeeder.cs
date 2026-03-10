using ConstructionManagement.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Helpers
{
    public static class DbSeeder
    {
        public static async Task SeedUsersAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<Data.AppDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

            await dbContext.Database.MigrateAsync(cancellationToken);

            var existingUsers = await dbContext.Users.ToListAsync(cancellationToken);
            var hasChanges = false;

            foreach (var user in existingUsers)
            {
                if (string.IsNullOrWhiteSpace(user.Name))
                {
                    user.Name = user.Username;
                    hasChanges = true;
                }

                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    user.Email = $"{user.Username}@construction.local";
                    hasChanges = true;
                }
            }

            if (hasChanges)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            if (existingUsers.Any())
            {
                return;
            }

            dbContext.Users.AddRange(
                new User
                {
                    UserId = Guid.NewGuid(),
                    Name = "Admin",
                    Username = "admin",
                    Email = "admin@construction.local",
                    PhoneNumber = null,
                    PasswordHash = passwordHasher.Hash("Admin123!"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Name = "Project Manager",
                    Username = "pm",
                    Email = "pm@construction.local",
                    PhoneNumber = null,
                    PasswordHash = passwordHasher.Hash("Pm123!"),
                    Role = "PM",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Name = "Contractor",
                    Username = "contractor",
                    Email = "contractor@construction.local",
                    PhoneNumber = null,
                    PasswordHash = passwordHasher.Hash("Contractor123!"),
                    Role = "Contractor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
