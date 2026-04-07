using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Tasks
{
    /// <summary>
    /// Summary task data returned by list endpoints.
    /// </summary>
    public class TaskItemListDto
    {
        public Guid TaskItemId { get; set; }

        public Guid ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string ProjectName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string? Category { get; set; }

        public decimal? EstimatedHours { get; set; }

        public Guid? AssignedUserId { get; set; }

        public string? AssignedUserName { get; set; }

        public List<Guid> AssignedUserIds { get; set; } = [];

        public List<TaskAssigneeDto> AssignedUsers { get; set; } = [];

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed task data returned by single-item endpoints.
    /// </summary>
    public class TaskItemDetailDto
    {
        public Guid TaskItemId { get; set; }

        public Guid ProjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string? Category { get; set; }

        public decimal? EstimatedHours { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string ProjectAddress { get; set; } = string.Empty;

        public Guid? AssignedUserId { get; set; }

        public string? AssignedUserName { get; set; }

        public List<Guid> AssignedUserIds { get; set; } = [];

        public List<TaskAssigneeDto> AssignedUsers { get; set; } = [];

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<TaskAttachmentDto> Attachments { get; set; } = [];
    }

    public class TaskAttachmentDto
    {
        public Guid AttachmentId { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; }
    }

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
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public string Priority { get; set; } = "Medium";

        public string? Category { get; set; }

        [Range(typeof(decimal), "0.5", "9999")]
        public decimal? EstimatedHours { get; set; }

        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public Guid? AssignedUserId { get; set; }

        public List<Guid>? AssignedUserIds { get; set; }
    }

    /// <summary>
    /// Request body used to generate an AI task draft from a field description.
    /// </summary>
    public class AiTaskDraftRequestDto
    {
        [Required]
        [MinLength(10)]
        public required string SiteDescription { get; set; }
    }

    /// <summary>
    /// Structured task draft suggested by the AI assistant.
    /// </summary>
    public class AiTaskDraftSuggestionDto
    {
        public string Title { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public string Priority { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public decimal EstimatedHours { get; set; }

        public List<string> ExecutionSteps { get; set; } = [];
    }

    public class UpdateTaskStatusDto
    {
        [Required]
        [MinLength(1)]
        public required string Status { get; set; }
    }

    /// <summary>
    /// Request body used to update an existing task item.
    /// </summary>
    public class UpdateTaskItemDto
    {
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        public string? Description { get; set; }

        public string Priority { get; set; } = "Medium";

        public string? Category { get; set; }

        [Range(typeof(decimal), "0.5", "9999")]
        public decimal? EstimatedHours { get; set; }

        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        public Guid? AssignedUserId { get; set; }

        public List<Guid>? AssignedUserIds { get; set; }
    }

    /// <summary>
    /// Request body used by a contractor to notify admin or PM about an issue or handoff.
    /// </summary>
    public class NotifyTaskAdminDto
    {
        [Required]
        [MinLength(1)]
        public required string Topic { get; set; }

        [Required]
        [MinLength(3)]
        public required string Message { get; set; }
    }

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
                Priority = taskItem.Priority,
                Category = taskItem.Category,
                EstimatedHours = taskItem.EstimatedHours,
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
                Priority = taskItem.Priority,
                Category = taskItem.Category,
                EstimatedHours = taskItem.EstimatedHours,
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
