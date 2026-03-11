using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.DTOs.Notifications
{
    /// <summary>
    /// Request body used to send an email update for a task team.
    /// </summary>
    public class SendTaskEmailDto
    {
        /// <summary>
        /// Optional custom email message. If omitted, the backend builds a default task summary.
        /// </summary>
        [MaxLength(4000)]
        public string? Message { get; set; }
    }

    /// <summary>
    /// Email sending summary returned after notifying assigned users.
    /// </summary>
    public class TaskEmailResultDto
    {
        public Guid TaskItemId { get; set; }

        public string TaskTitle { get; set; } = string.Empty;

        public int AttemptedCount { get; set; }

        public int SentCount { get; set; }

        public List<string> SentRecipients { get; set; } = [];

        public List<string> SkippedRecipients { get; set; } = [];

        public List<string> FailedRecipients { get; set; } = [];
    }
}
