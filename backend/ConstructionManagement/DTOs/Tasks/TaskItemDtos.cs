using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Tasks
{
    /// <summary>
    /// Summary task data returned by list endpoints.
    /// </summary>
    public class TaskItemListDto
    {
        /// <summary>
        /// Unique task identifier.
        /// </summary>
        public Guid TaskItemId { get; set; }

        /// <summary>
        /// Parent project identifier.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Short title of the task.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional detailed description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Current task status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Planned due date for the task.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// UTC timestamp when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed task data returned by single-item endpoints.
    /// </summary>
    public class TaskItemDetailDto
    {
        /// <summary>
        /// Unique task identifier.
        /// </summary>
        public Guid TaskItemId { get; set; }

        /// <summary>
        /// Parent project identifier.
        /// </summary>
        public Guid ProjectId { get; set; }

        /// <summary>
        /// Short title of the task.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Optional detailed description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Current task status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Planned due date for the task.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// UTC timestamp when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request body used to create a new task item.
    /// </summary>
    public class CreateTaskItemDto
    {
        /// <summary>
        /// Short title of the task.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        /// <summary>
        /// Optional detailed description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Planned due date for the task.
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }
    }

    /// <summary>
    /// Request body used to update task status.
    /// </summary>
    public class UpdateTaskStatusDto
    {
        /// <summary>
        /// New task status. Allowed values: Todo, Doing, Done.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Status { get; set; }
    }

    /// <summary>
    /// Request body used to update an existing task item.
    /// </summary>
    public class UpdateTaskItemDto
    {
        /// <summary>
        /// Updated short title of the task.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        /// <summary>
        /// Updated detailed description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Updated due date for the task.
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }
    }

    public static class TaskItemDtoMappings
    {
        public static TaskItemListDto ToListDto(this TaskItem taskItem)
        {
            return new TaskItemListDto
            {
                TaskItemId = taskItem.TaskItemId,
                ProjectId = taskItem.ProjectId,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Status = taskItem.Status,
                DueDate = taskItem.DueDate,
                CreatedAt = taskItem.CreatedAt
            };
        }

        public static TaskItemDetailDto ToDetailDto(this TaskItem taskItem)
        {
            return new TaskItemDetailDto
            {
                TaskItemId = taskItem.TaskItemId,
                ProjectId = taskItem.ProjectId,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Status = taskItem.Status,
                DueDate = taskItem.DueDate,
                CreatedAt = taskItem.CreatedAt
            };
        }
    }
}
