using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Variations
{
    /// <summary>
    /// Summary variation data returned by list endpoints.
    /// </summary>
    public class VariationListDto
    {
        public Guid VariationId { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed variation data returned by single-item endpoints.
    /// </summary>
    public class VariationDetailDto
    {
        public Guid VariationId { get; set; }
        public Guid ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<VariationStatusHistoryDto> StatusHistory { get; set; } = [];
    }

    public class VariationStatusHistoryDto
    {
        public Guid VariationStatusHistoryId { get; set; }
        public string FromStatus { get; set; } = string.Empty;
        public string ToStatus { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public Guid? ActorUserId { get; set; }
        public string ActorName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request body used to create a new variation.
    /// </summary>
    public class CreateVariationDto
    {
        /// <summary>
        /// Short title of the variation.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        /// <summary>
        /// Optional detailed description of the variation.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Proposed variation amount.
        /// </summary>
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Request body used to update variation status.
    /// </summary>
    public class UpdateVariationStatusDto
    {
        /// <summary>
        /// New variation status. Allowed values: Draft, Submitted, Approved, Rejected, NeedInfo.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Status { get; set; }

        public string? Comment { get; set; }
    }

    /// <summary>
    /// Request body used to update an existing variation.
    /// </summary>
    public class UpdateVariationDto
    {
        /// <summary>
        /// Updated short title of the variation.
        /// </summary>
        [Required]
        [MinLength(1)]
        public required string Title { get; set; }

        /// <summary>
        /// Updated detailed description of the variation.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Updated proposed variation amount.
        /// </summary>
        [Range(typeof(decimal), "0", "79228162514264337593543950335")]
        public decimal Amount { get; set; }
    }

    public static class VariationDtoMappings
    {
        public static VariationListDto ToListDto(this Variation variation)
        {
            return new VariationListDto
            {
                VariationId = variation.VariationId,
                ProjectId = variation.ProjectId,
                Title = variation.Title,
                Description = variation.Description,
                Amount = variation.Amount,
                Status = variation.Status,
                CreatedAt = variation.CreatedAt
            };
        }

        public static VariationDetailDto ToDetailDto(this Variation variation)
        {
            return new VariationDetailDto
            {
                VariationId = variation.VariationId,
                ProjectId = variation.ProjectId,
                Title = variation.Title,
                Description = variation.Description,
                Amount = variation.Amount,
                Status = variation.Status,
                CreatedAt = variation.CreatedAt,
                StatusHistory = variation.StatusHistory
                    .OrderByDescending(history => history.CreatedAt)
                    .Select(history => new VariationStatusHistoryDto
                    {
                        VariationStatusHistoryId = history.VariationStatusHistoryId,
                        FromStatus = history.FromStatus,
                        ToStatus = history.ToStatus,
                        Comment = history.Comment,
                        ActorUserId = history.ActorUserId,
                        ActorName = history.ActorName,
                        CreatedAt = history.CreatedAt
                    })
                    .ToList()
            };
        }
    }
}
