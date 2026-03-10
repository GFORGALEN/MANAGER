using Microsoft.EntityFrameworkCore;
using ConstructionManagement.Entities;

namespace ConstructionManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Project> Projects { get; set; }

        public DbSet<TaskItem> TaskItems { get; set; }

        public DbSet<Variation> Variations { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .Property(project => project.Budget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<TaskItem>()
                .HasOne(taskItem => taskItem.Project)
                .WithMany(project => project.TaskItems)
                .HasForeignKey(taskItem => taskItem.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskItem>()
                .HasOne(taskItem => taskItem.AssignedUser)
                .WithMany(user => user.AssignedTasks)
                .HasForeignKey(taskItem => taskItem.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Variation>()
                .Property(variation => variation.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Variation>()
                .HasOne(variation => variation.Project)
                .WithMany(project => project.Variations)
                .HasForeignKey(variation => variation.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Attachment>()
                .HasOne(attachment => attachment.Project)
                .WithMany(project => project.Attachments)
                .HasForeignKey(attachment => attachment.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Username)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();
        }
    }
}
