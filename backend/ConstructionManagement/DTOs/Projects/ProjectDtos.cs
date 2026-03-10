using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Projects
{
    /// <summary>
    /// Summary project data returned by list endpoints.
    /// </summary>
    public class ProjectListDto
    {
        /// <summary>
        /// Unique project identifier.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Internal project code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Project name shown to users.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Project site address or location.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Customer or owner name.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Project status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the project was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed project data returned by single-item endpoints.
    /// </summary>
    public class ProjectDetailDto
    {
        /// <summary>
        /// Unique project identifier.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Internal project code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Project name shown to users.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Project site address or location.
        /// </summary>
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Longer project summary.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Customer or owner name.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Project lifecycle status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Approved budget value when available.
        /// </summary>
        public decimal? Budget { get; set; }

        /// <summary>
        /// Planned start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Planned finish date.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// UTC timestamp when the project was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request body used to create a new project.
    /// </summary>
    public class CreateProjectDto
    {
        /// <summary>
        /// Internal project code.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Code { get; set; }

        /// <summary>
        /// Project name shown in the system.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Name { get; set; }

        /// <summary>
        /// Project site address or location.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Address { get; set; }

        /// <summary>
        /// Longer project summary.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Customer or owner name.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Current project status.
        /// </summary>
        [Required]
        [MinLength(1)]
        public string Status { get; set; } = "Planning";

        /// <summary>
        /// Approved budget value when available.
        /// </summary>
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal? Budget { get; set; }

        /// <summary>
        /// Planned start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Planned finish date.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Request body used to update an existing project.
    /// </summary>
    public class UpdateProjectDto
    {
        /// <summary>
        /// Updated internal project code.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Code { get; set; }

        /// <summary>
        /// Updated project name.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Name { get; set; }

        /// <summary>
        /// Updated project address.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Address { get; set; }

        /// <summary>
        /// Updated project summary.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Updated customer or owner name.
        /// </summary>
        public string? ClientName { get; set; }

        /// <summary>
        /// Updated project status.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Status { get; set; }

        /// <summary>
        /// Updated approved budget value.
        /// </summary>
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal? Budget { get; set; }

        /// <summary>
        /// Updated planned start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Updated planned finish date.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    public static class ProjectDtoMappings
    {
        public static ProjectListDto ToListDto(this Project project)
        {
            return new ProjectListDto
            {
                ProjectId = project.ProjectId,
                Code = project.Code,
                Name = project.Name,
                Address = project.Address,
                ClientName = project.ClientName,
                Status = project.Status,
                CreatedAt = project.CreatedAt
            };
        }

        public static ProjectDetailDto ToDetailDto(this Project project)
        {
            return new ProjectDetailDto
            {
                ProjectId = project.ProjectId,
                Code = project.Code,
                Name = project.Name,
                Address = project.Address,
                Description = project.Description,
                ClientName = project.ClientName,
                Status = project.Status,
                Budget = project.Budget,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                CreatedAt = project.CreatedAt
            };
        }
    }
}
