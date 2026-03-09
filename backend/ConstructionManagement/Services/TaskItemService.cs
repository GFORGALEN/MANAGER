using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Tasks;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskItemService> _logger;

        public TaskItemService(AppDbContext context, ILogger<TaskItemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResultDto<TaskItemListDto>?> GetProjectTasksAsync(Guid projectId, TaskItemQueryDto query, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when querying tasks", projectId);
                return null;
            }

            var tasks = _context.TaskItems.Where(task => task.ProjectId == projectId);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = StatusValidators.ValidateTaskStatus(query.Status);
                tasks = tasks.Where(task => task.Status == status);
            }

            tasks = (query.SortBy?.ToLowerInvariant(), query.SortOrder?.ToLowerInvariant()) switch
            {
                ("title", "desc") => tasks.OrderByDescending(task => task.Title),
                ("title", _) => tasks.OrderBy(task => task.Title),
                ("duedate", "desc") => tasks.OrderByDescending(task => task.DueDate),
                ("duedate", _) => tasks.OrderBy(task => task.DueDate),
                _ => tasks.OrderByDescending(task => task.CreatedAt)
            };

            var (items, totalCount) = await tasks
                .Select(task => task.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<TaskItemListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<TaskItemDetailDto?> GetTaskAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems.FindAsync([id], cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found", id);
                return null;
            }

            return task.ToDetailDto();
        }

        public async Task<TaskItemDetailDto?> CreateTaskAsync(Guid projectId, CreateTaskItemDto request, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when creating task", projectId);
                return null;
            }

            var task = new TaskItem
            {
                TaskItemId = Guid.NewGuid(),
                ProjectId = projectId,
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Status = "Todo",
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task {TaskId} created under project {ProjectId}", task.TaskItemId, projectId);
            return task.ToDetailDto();
        }

        public async Task<TaskItemDetailDto?> UpdateTaskAsync(Guid id, UpdateTaskItemDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems.FindAsync([id], cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for update", id);
                return null;
            }

            task.Title = request.Title.Trim();
            task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            task.DueDate = request.DueDate;

            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }

        public async Task<TaskItemDetailDto?> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems.FindAsync([id], cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for status update", id);
                return null;
            }

            task.Status = StatusValidators.ValidateTaskStatus(request.Status);
            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }
    }
}
