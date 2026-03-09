namespace ConstructionManagement.Entities
{
    public class Attachment
    {
        public Guid AttachmentId { get; set; }

        public Guid ProjectId { get; set; }

        public required string FileName { get; set; }

        public required string FilePath { get; set; }

        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;
    }
}
