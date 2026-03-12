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
        /// Related project name.
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Optional detailed description of the task.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Current task status.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Assigned worker identifier when the task is assigned.
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Assigned worker display name when available.
        /// </summary>
        public string? AssignedUserName { get; set; }

        /// <summary>
        /// All assigned user IDs.
        /// </summary>
        public List<Guid> AssignedUserIds { get; set; } = [];

        /// <summary>
        /// All assigned users for this task.
        /// </summary>
        public List<TaskAssigneeDto> AssignedUsers { get; set; } = [];

        /// <summary>
        /// Planned start date for the task.
        /// </summary>
        public DateTime StartDate { get; set; }

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
        /// Related project name.
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// Related project address.
        /// </summary>
        public string ProjectAddress { get; set; } = string.Empty;

        /// <summary>
        /// Assigned worker identifier when the task is assigned.
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Assigned worker display name when available.
        /// </summary>
        public string? AssignedUserName { get; set; }

        /// <summary>
        /// All assigned user IDs.
        /// </summary>
        public List<Guid> AssignedUserIds { get; set; } = [];

        /// <summary>
        /// All assigned users for this task.
        /// </summary>
        public List<TaskAssigneeDto> AssignedUsers { get; set; } = [];

        /// <summary>
        /// Planned start date for the task.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Planned due date for the task.
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// UTC timestamp when the task was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Project attachments available to the worker for reference.
        /// </summary>
        public List<TaskAttachmentDto> Attachments { get; set; } = [];
    }

    /// <summary>
    /// Attachment summary nested under task detail responses.
    /// </summary>
    public class TaskAttachmentDto
    {
        public Guid AttachmentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// Minimal assignee summary nested under task responses.
    /// </summary>
    public class TaskAssigneeDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
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
        /// Planned start date for the task. If omitted, the backend uses the current UTC time.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Planned due date for the task.
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Optional contractor user ID assigned to the task.
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Optional list of assigned user IDs for collaborative work.
        /// </summary>
        public List<Guid>? AssignedUserIds { get; set; }
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
        /// Updated planned start date for the task. If omitted, the backend keeps the existing start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Updated due date for the task.
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Optional contractor user ID assigned to the task.
        /// </summary>
        public Guid? AssignedUserId { get; set; }

        /// <summary>
        /// Optional list of assigned user IDs for collaborative work.
        /// </summary>
        public List<Guid>? AssignedUserIds { get; set; }
    }

    /// <summary>
    /// Request body used by a contractor to notify admin or PM about an issue or handoff.
    /// </summary>
    public class NotifyTaskAdminDto
    {
        /// <summary>
        /// Communication topic. Allowed values: Issue, Completion.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Topic { get; set; }

        /// <summary>
        /// Free-form message from the contractor.
        /// </summary>
        [Required]
        [MinLength(3)]
        public required string Message { get; set; }
    }

    /// <summary>
    /// Summary result returned after notifying admin or PM from a task.
    /// </summary>
    public class TaskAdminNotificationResultDto
    {
        public Guid TaskItemId { get; set; }

        public string Topic { get; set; } = string.Empty;

        public int AttemptedCount { get; set; }

        public int SentCount { get; set; }

        public List<string> SentRecipients { get; set; } = [];

        public List<string> FailedRecipients { get; set; } = [];
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
                ProjectName = taskItem.Project?.Name ?? string.Empty,
                Description = taskItem.Description,
                Status = taskItem.Status,
                AssignedUserId = taskItem.AssignedUserId,
                AssignedUserName = taskItem.AssignedUser?.Name,
                AssignedUserIds = taskItem.TaskAssignments
                    .Select(taskAssignment => taskAssignment.UserId)
                    .ToList(),
                AssignedUsers = taskItem.TaskAssignments
                    .Select(taskAssignment => new TaskAssigneeDto
                    {
                        UserId = taskAssignment.UserId,
                        Name = taskAssignment.User.Name,
                        Email = taskAssignment.User.Email,
                        Role = taskAssignment.User.Role
                    })
                    .ToList(),
                StartDate = taskItem.StartDate,
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
                ProjectName = taskItem.Project?.Name ?? string.Empty,
                ProjectAddress = taskItem.Project?.Address ?? string.Empty,
                AssignedUserId = taskItem.AssignedUserId,
                AssignedUserName = taskItem.AssignedUser?.Name,
                AssignedUserIds = taskItem.TaskAssignments
                    .Select(taskAssignment => taskAssignment.UserId)
                    .ToList(),
                AssignedUsers = taskItem.TaskAssignments
                    .Select(taskAssignment => new TaskAssigneeDto
                    {
                        UserId = taskAssignment.UserId,
                        Name = taskAssignment.User.Name,
                        Email = taskAssignment.User.Email,
                        Role = taskAssignment.User.Role
                    })
                    .ToList(),
                StartDate = taskItem.StartDate,
                DueDate = taskItem.DueDate,
                CreatedAt = taskItem.CreatedAt,
                Attachments = taskItem.Project?.Attachments
                    .Select(attachment => new TaskAttachmentDto
                    {
                        AttachmentId = attachment.AttachmentId,
                        FileName = attachment.FileName,
                        FilePath = attachment.FilePath,
                        ContentType = attachment.ContentType,
                        FileSize = attachment.FileSize,
                        UploadedAt = attachment.UploadedAt
                    })
                    .ToList() ?? []
            };
        }
    }
}
