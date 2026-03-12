using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Attachments;
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
        private readonly IAttachmentService _attachmentService;
        private readonly IWebHostEnvironment _environment;

        public MyTasksController(ITaskItemService taskItemService, IAttachmentService attachmentService, IWebHostEnvironment environment)
        {
            _taskItemService = taskItemService;
            _attachmentService = attachmentService;
            _environment = environment;
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

        /// <summary>
        /// Sends a worker issue or completion notification to active admins and project managers.
        /// </summary>
        /// <param name="id">The task identifier.</param>
        /// <param name="request">The notification topic and message.</param>
        /// <returns>A delivery summary if the task was found.</returns>
        [HttpPost("{id:guid}/notify-admin")]
        public async Task<ActionResult<TaskAdminNotificationResultDto>> NotifyAdmin(Guid id, [FromBody] NotifyTaskAdminDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _taskItemService.NotifyMyTaskAdminAsync(GetCurrentUserId(), id, request, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        /// <summary>
        /// Uploads a field attachment for a task assigned to the current contractor.
        /// The file is stored under the parent project so managers can review it in the attachment workspace.
        /// </summary>
        /// <param name="id">The task identifier.</param>
        /// <param name="file">The uploaded file.</param>
        /// <returns>The stored attachment metadata if the task was found.</returns>
        [HttpPost("{id:guid}/attachments/upload")]
        public async Task<ActionResult<AttachmentDetailDto>> UploadMyTaskAttachment(Guid id, IFormFile file, CancellationToken cancellationToken)
        {
            var task = await _taskItemService.GetMyTaskAsync(GetCurrentUserId(), id, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            var attachment = await _attachmentService.UploadAttachmentAsync(task.ProjectId, file, cancellationToken);
            if (attachment is null)
            {
                return NotFound();
            }

            return Ok(attachment);
        }

        /// <summary>
        /// Streams an attachment that belongs to the current contractor's assigned task.
        /// </summary>
        /// <param name="id">The task identifier.</param>
        /// <param name="attachmentId">The attachment identifier.</param>
        /// <returns>The physical attachment file if found and authorized.</returns>
        [HttpGet("{id:guid}/attachments/{attachmentId:guid}/content")]
        public async Task<IActionResult> GetMyTaskAttachmentContent(Guid id, Guid attachmentId, CancellationToken cancellationToken)
        {
            var task = await _taskItemService.GetMyTaskAsync(GetCurrentUserId(), id, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            if (!task.Attachments.Any(attachment => attachment.AttachmentId == attachmentId))
            {
                return NotFound();
            }

            var attachment = await _attachmentService.GetAttachmentAsync(attachmentId, cancellationToken);
            if (attachment is null)
            {
                return NotFound();
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            var contentType = string.IsNullOrWhiteSpace(attachment.ContentType) ? "application/octet-stream" : attachment.ContentType;
            return PhysicalFile(fullPath, contentType, attachment.FileName, enableRangeProcessing: true);
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
