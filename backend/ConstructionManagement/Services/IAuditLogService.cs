using ConstructionManagement.DTOs.Audit;
using ConstructionManagement.DTOs.Common;

namespace ConstructionManagement.Services
{
    public interface IAuditLogService
    {
        Task<PagedResultDto<AuditLogListDto>> GetAuditLogsAsync(AuditLogQueryDto query, CancellationToken cancellationToken = default);

        Task RecordAsync(Guid? actorUserId, string entityType, Guid entityId, string action, string? fieldName, string? oldValue, string? newValue, string? summary, CancellationToken cancellationToken = default);
    }
}
