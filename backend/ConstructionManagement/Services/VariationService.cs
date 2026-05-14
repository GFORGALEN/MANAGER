using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Variations;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class VariationService : IVariationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VariationService> _logger;
        private readonly INotificationService _notificationService;
        private readonly IAuditLogService _auditLogService;

        public VariationService(AppDbContext context, ILogger<VariationService> logger, INotificationService notificationService, IAuditLogService auditLogService)
        {
            _context = context;
            _logger = logger;
            _notificationService = notificationService;
            _auditLogService = auditLogService;
        }

        public async Task<PagedResultDto<VariationListDto>?> GetProjectVariationsAsync(Guid projectId, VariationQueryDto query, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when querying variations", projectId);
                return null;
            }

            var variations = _context.Variations.Where(variation => variation.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = StatusValidators.ValidateVariationStatus(query.Status);
                variations = variations.Where(variation => variation.Status == status);
            }

            variations = (query.SortBy?.ToLowerInvariant(), query.SortOrder?.ToLowerInvariant()) switch
            {
                ("title", "desc") => variations.OrderByDescending(variation => variation.Title),
                ("title", _) => variations.OrderBy(variation => variation.Title),
                ("amount", "asc") => variations.OrderBy(variation => variation.Amount),
                ("amount", "desc") => variations.OrderByDescending(variation => variation.Amount),
                _ => variations.OrderByDescending(variation => variation.CreatedAt)
            };

            var (items, totalCount) = await variations
                .Select(variation => variation.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<VariationListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<VariationDetailDto?> GetVariationAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var variation = await _context.Variations
                .Include(item => item.StatusHistory)
                .FirstOrDefaultAsync(item => item.VariationId == id, cancellationToken);
            if (variation is null)
            {
                _logger.LogWarning("Variation {VariationId} was not found", id);
                return null;
            }

            return variation.ToDetailDto();
        }

        public async Task<VariationDetailDto?> CreateVariationAsync(Guid projectId, CreateVariationDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when creating variation", projectId);
                return null;
            }

            var variation = new Variation
            {
                VariationId = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Amount = request.Amount,
                Status = "Draft",
                CreatedAt = DateTime.UtcNow
            };

            _context.Variations.Add(variation);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Variation", variation.VariationId, "Created", null, null, variation.Title, $"Variation created in project {projectId}", cancellationToken);
            await _notificationService.CreateForRolesAsync(
                ["Admin", "PM"],
                "New variation created",
                $"{variation.Title} is ready for review.",
                "Variation",
                "Variation",
                variation.VariationId,
                cancellationToken);
            return variation.ToDetailDto();
        }

        public async Task<VariationDetailDto?> UpdateVariationAsync(Guid id, UpdateVariationDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            var variation = await _context.Variations.FindAsync([id], cancellationToken);
            if (variation is null)
            {
                _logger.LogWarning("Variation {VariationId} was not found for update", id);
                return null;
            }

            variation.Title = request.Title.Trim();
            variation.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            variation.Amount = request.Amount;

            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Variation", variation.VariationId, "Updated", null, null, variation.Title, "Variation details updated", cancellationToken);
            return variation.ToDetailDto();
        }

        public async Task<VariationDetailDto?> UpdateVariationStatusAsync(Guid id, UpdateVariationStatusDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            var variation = await _context.Variations
                .Include(item => item.StatusHistory)
                .FirstOrDefaultAsync(item => item.VariationId == id, cancellationToken);
            if (variation is null)
            {
                _logger.LogWarning("Variation {VariationId} was not found for status update", id);
                return null;
            }

            var oldStatus = variation.Status;
            variation.Status = StatusValidators.ValidateVariationStatus(request.Status);
            var actorName = await ResolveActorNameAsync(actorUserId, cancellationToken);
            variation.StatusHistory.Add(new VariationStatusHistory
            {
                VariationStatusHistoryId = Guid.NewGuid(),
                VariationId = variation.VariationId,
                FromStatus = oldStatus,
                ToStatus = variation.Status,
                Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
                ActorUserId = actorUserId,
                ActorName = actorName,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Variation", variation.VariationId, "StatusChanged", "Status", oldStatus, variation.Status, request.Comment, cancellationToken);
            await _notificationService.CreateForRolesAsync(
                ["Admin", "PM"],
                "Variation workflow updated",
                $"{variation.Title} moved from {oldStatus} to {variation.Status}.",
                "Variation",
                "Variation",
                variation.VariationId,
                cancellationToken);
            _logger.LogInformation("Variation {VariationId} status changed to {Status}", id, variation.Status);
            return variation.ToDetailDto();
        }

        private async Task<string> ResolveActorNameAsync(Guid? actorUserId, CancellationToken cancellationToken)
        {
            if (!actorUserId.HasValue)
            {
                return "System";
            }

            return await _context.Users
                .Where(user => user.UserId == actorUserId.Value)
                .Select(user => user.Name)
                .FirstOrDefaultAsync(cancellationToken) ?? "Unknown user";
        }
    }
}
