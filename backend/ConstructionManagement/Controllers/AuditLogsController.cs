using ConstructionManagement.DTOs.Audit;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,PM")]
    public class AuditLogsController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogsController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("api/audit-logs")]
        public async Task<ActionResult<PagedResultDto<AuditLogListDto>>> GetAuditLogs([FromQuery] AuditLogQueryDto query, CancellationToken cancellationToken)
        {
            return Ok(await _auditLogService.GetAuditLogsAsync(query, cancellationToken));
        }
    }
}
