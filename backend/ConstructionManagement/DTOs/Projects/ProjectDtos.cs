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
        /// Project name shown to users.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Project site address or location.
        /// </summary>
        public string Address { get; set; } = string.Empty;

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
        /// Project name shown to users.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Project site address or location.
        /// </summary>
        public string Address { get; set; } = string.Empty;

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
    }

    /// <summary>
    /// Request body used to update an existing project.
    /// </summary>
    public class UpdateProjectDto
    {
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
    }

    public static class ProjectDtoMappings
    {
        public static ProjectListDto ToListDto(this Project project)
        {
            return new ProjectListDto
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Address = project.Address,
                CreatedAt = project.CreatedAt
            };
        }

        public static ProjectDetailDto ToDetailDto(this Project project)
        {
            return new ProjectDetailDto
            {
                ProjectId = project.ProjectId,
                Name = project.Name,
                Address = project.Address,
                CreatedAt = project.CreatedAt
            };
        }
    }
}
