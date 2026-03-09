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

            if (await dbContext.Users.AnyAsync(cancellationToken))
            {
                return;
            }

            dbContext.Users.AddRange(
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "admin",
                    PasswordHash = passwordHasher.Hash("Admin123!"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "pm",
                    PasswordHash = passwordHasher.Hash("Pm123!"),
                    Role = "PM",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = Guid.NewGuid(),
                    Username = "contractor",
                    PasswordHash = passwordHasher.Hash("Contractor123!"),
                    Role = "Contractor",
                    CreatedAt = DateTime.UtcNow
                });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
