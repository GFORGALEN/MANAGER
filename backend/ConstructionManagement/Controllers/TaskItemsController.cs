using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Notifications;
using ConstructionManagement.DTOs.Tasks;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides endpoints for managing task items under projects.
    /// </summary>
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly ITaskItemService _taskItemService;

        public TaskItemsController(ITaskItemService taskItemService)
        {
            _taskItemService = taskItemService;
        }

        /// <summary>
        /// Gets all tasks that belong to a specific project.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>All tasks under the specified project.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpGet("api/projects/{projectId:guid}/tasks")]
        public async Task<ActionResult<PagedResultDto<TaskItemListDto>>> GetProjectTasks(Guid projectId, [FromQuery] TaskItemQueryDto query, CancellationToken cancellationToken)
        {
            var tasks = await _taskItemService.GetProjectTasksAsync(projectId, query, cancellationToken);
            if (tasks is null)
            {
                return NotFound();
            }

            return Ok(tasks);
        }

        /// <summary>
        /// Gets a single task by its unique identifier.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <returns>The matching task if found.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpGet("api/tasks/{id:guid}")]
        public async Task<ActionResult<TaskItemDetailDto>> GetTask(Guid id, CancellationToken cancellationToken)
        {
            var task = await _taskItemService.GetTaskAsync(id, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        /// <summary>
        /// Creates a new task under a specific project.
        /// </summary>
        /// <param name="projectId">The project ID that the task belongs to.</param>
        /// <param name="request">The task data to create.</param>
        /// <returns>The newly created task.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpPost("api/projects/{projectId:guid}/tasks")]
        public async Task<ActionResult<TaskItemDetailDto>> CreateTask(Guid projectId, [FromBody] CreateTaskItemDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var task = await _taskItemService.CreateTaskAsync(projectId, request, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetTask), new { id = task.TaskItemId }, task);
        }

        /// <summary>
        /// Updates editable fields of an existing task.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="request">The updated task data.</param>
        /// <returns>The updated task.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpPut("api/tasks/{id:guid}")]
        public async Task<ActionResult<TaskItemDetailDto>> UpdateTask(Guid id, [FromBody] UpdateTaskItemDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var task = await _taskItemService.UpdateTaskAsync(id, request, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        /// <summary>
        /// Updates only the status of a task.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="request">The new status value. Allowed values: Todo, Doing, Done.</param>
        /// <returns>The updated task.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpPatch("api/tasks/{id:guid}/status")]
        public async Task<ActionResult<TaskItemDetailDto>> UpdateTaskStatus(Guid id, [FromBody] UpdateTaskStatusDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var task = await _taskItemService.UpdateTaskStatusAsync(id, request, cancellationToken);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        /// <summary>
        /// Sends an SMS update to all active users assigned to a task.
        /// </summary>
        /// <param name="id">The task ID.</param>
        /// <param name="request">Optional custom SMS body. If omitted, the backend sends a default task summary.</param>
        /// <returns>A delivery summary including sent and skipped recipients.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpPost("api/tasks/{id:guid}/notify-sms")]
        public async Task<ActionResult<TaskSmsResultDto>> SendTaskSms(Guid id, [FromBody] SendTaskSmsDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _taskItemService.SendTaskSmsAsync(id, request.Message, cancellationToken);
            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
