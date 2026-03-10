using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Tasks;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides contractor self-service task endpoints for the current authenticated user.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Contractor")]
    [Route("api/my/tasks")]
    public class MyTasksController : ControllerBase
    {
        private readonly ITaskItemService _taskItemService;

        public MyTasksController(ITaskItemService taskItemService)
        {
            _taskItemService = taskItemService;
        }

        /// <summary>
        /// Gets all tasks assigned to the current contractor.
        /// </summary>
        /// <param name="query">Optional task filters and paging.</param>
        /// <returns>The current contractor's assigned tasks.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<TaskItemListDto>>> GetMyTasks([FromQuery] TaskItemQueryDto query, CancellationToken cancellationToken)
        {
            return Ok(await _taskItemService.GetMyTasksAsync(GetCurrentUserId(), query, cancellationToken));
        }

        /// <summary>
        /// Gets a single assigned task that belongs to the current contractor.
        /// </summary>
        /// <param name="id">The task identifier.</param>
        /// <returns>The assigned task detail if found.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TaskItemDetailDto>> GetMyTask(Guid id, CancellationToken cancellationToken)
        {
            var task = await _taskItemService.GetMyTaskAsync(GetCurrentUserId(), id, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        /// <summary>
        /// Updates the status of a task assigned to the current contractor.
        /// </summary>
        /// <param name="id">The task identifier.</param>
        /// <param name="request">The new task status.</param>
        /// <returns>The updated task detail if found.</returns>
        [HttpPatch("{id:guid}/status")]
        public async Task<ActionResult<TaskItemDetailDto>> UpdateMyTaskStatus(Guid id, [FromBody] UpdateTaskStatusDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var task = await _taskItemService.UpdateMyTaskStatusAsync(GetCurrentUserId(), id, request, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("Authenticated user identifier is missing.");
            }

            return userId;
        }
    }
}
