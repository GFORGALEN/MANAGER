using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.DTOs.Common
{
    public class ProjectQueryDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Keyword { get; set; }

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }

    public class TaskItemQueryDto
    {
        public string? Status { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }

    public class VariationQueryDto
    {
        public string? Status { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }

    public class AttachmentQueryDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? SortBy { get; set; }

        public string? SortOrder { get; set; }
    }

    public class UserQueryDto
    {
        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;

        public string? Keyword { get; set; }

        public string? Role { get; set; }

        public bool? IsActive { get; set; }
    }
}
