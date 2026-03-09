using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Attachments
{
    /// <summary>
    /// Summary attachment data returned by list endpoints.
    /// </summary>
    public class AttachmentListDto
    {
        public Guid AttachmentId { get; set; }
        public Guid ProjectId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// Detailed attachment data returned by single-item endpoints.
    /// </summary>
    public class AttachmentDetailDto
    {
        public Guid AttachmentId { get; set; }
        public Guid ProjectId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    /// <summary>
    /// Request body used to create a new attachment metadata record.
    /// </summary>
    public class CreateAttachmentDto
    {
        /// <summary>
        /// Original file name shown to users.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string FileName { get; set; }

        /// <summary>
        /// Stored file path or storage key.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string FilePath { get; set; }

        /// <summary>
        /// Optional MIME content type such as application/pdf or image/png.
        /// </summary>
        public string? ContentType { get; set; }
    }

    /// <summary>
    /// Request body used to update attachment metadata.
    /// </summary>
    public class UpdateAttachmentDto
    {
        [Required]
        [MinLength(1)]
        public required string FileName { get; set; }

        [Required]
        [MinLength(1)]
        public required string FilePath { get; set; }

        public string? ContentType { get; set; }
    }

    public static class AttachmentDtoMappings
    {
        public static AttachmentListDto ToListDto(this Attachment attachment)
        {
            return new AttachmentListDto
            {
                AttachmentId = attachment.AttachmentId,
                ProjectId = attachment.ProjectId,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt
            };
        }

        public static AttachmentDetailDto ToDetailDto(this Attachment attachment)
        {
            return new AttachmentDetailDto
            {
                AttachmentId = attachment.AttachmentId,
                ProjectId = attachment.ProjectId,
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                ContentType = attachment.ContentType,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt
            };
        }
    }
}
