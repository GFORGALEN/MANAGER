using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Audit;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDto<AuditLogListDto>> GetAuditLogsAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default)
        {
            var logs = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.EntityType))
            {
                var entityType = query.EntityType.Trim();
                logs = logs.Where(log => log.EntityType == entityType);
            }

            if (query.EntityId.HasValue)
            {
                logs = logs.Where(log => log.EntityId == query.EntityId.Value);
            }

            var (items, totalCount) = await logs
                .OrderByDescending(log => log.CreatedAt)
                .Select(log => log.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<AuditLogListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task RecordAsync(Guid? actorUserId, string entityType, Guid entityId, string action, string? fieldName, string? oldValue, string? newValue, string? summary, CancellationToken cancellationToken = default)
        {
            var actorName = "System";
            if (actorUserId.HasValue)
            {
                actorName = await _context.Users
                    .Where(user => user.UserId == actorUserId.Value)
                    .Select(user => user.Name)
                    .FirstOrDefaultAsync(cancellationToken) ?? "Unknown user";
            }

            _context.AuditLogs.Add(new AuditLog
            {
                AuditLogId = Guid.NewGuid(),
                ActorUserId = actorUserId,
                ActorName = actorName,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                Summary = summary,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
