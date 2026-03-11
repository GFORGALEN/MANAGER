using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.DTOs.Notifications
{
    /// <summary>
    /// Request body used to send an SMS update for a task team.
    /// </summary>
    public class SendTaskSmsDto
    {
        /// <summary>
        /// Optional custom message. If omitted, the backend builds a default task summary.
        /// </summary>
        [MaxLength(1600)]
        public string? Message { get; set; }
    }

    /// <summary>
    /// SMS sending summary returned after notifying assigned users.
    /// </summary>
    public class TaskSmsResultDto
    {
        /// <summary>
        /// Related task identifier.
        /// </summary>
        public Guid TaskItemId { get; set; }

        /// <summary>
        /// Related task title.
        /// </summary>
        public string TaskTitle { get; set; } = string.Empty;

        /// <summary>
        /// Number of recipients attempted.
        /// </summary>
        public int AttemptedCount { get; set; }

        /// <summary>
        /// Number of successfully accepted messages.
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// Users skipped because phone data was missing or invalid.
        /// </summary>
        public List<string> SkippedRecipients { get; set; } = [];

        /// <summary>
        /// Users that failed at provider level.
        /// </summary>
        public List<string> FailedRecipients { get; set; } = [];
    }
}
