using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Tasks;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using System.ComponentModel.DataAnnotations;
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
                .Include(task => task.Project)
                .Include(task => task.AssignedUser)
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
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .ThenInclude(project => project.Attachments)
                .Include(item => item.AssignedUser)
                .FirstOrDefaultAsync(item => item.TaskItemId == id, cancellationToken);
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
                AssignedUserId = await ValidateAssignedUserAsync(request.AssignedUserId, cancellationToken),
                CreatedAt = DateTime.UtcNow
            };

            _context.TaskItems.Add(task);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Task {TaskId} created under project {ProjectId}", task.TaskItemId, projectId);
            return await GetTaskAsync(task.TaskItemId, cancellationToken);
        }

        public async Task<TaskItemDetailDto?> UpdateTaskAsync(Guid id, UpdateTaskItemDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .ThenInclude(project => project.Attachments)
                .Include(item => item.AssignedUser)
                .FirstOrDefaultAsync(item => item.TaskItemId == id, cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for update", id);
                return null;
            }

            task.Title = request.Title.Trim();
            task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            task.DueDate = request.DueDate;
            task.AssignedUserId = await ValidateAssignedUserAsync(request.AssignedUserId, cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }

        public async Task<TaskItemDetailDto?> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .ThenInclude(project => project.Attachments)
                .Include(item => item.AssignedUser)
                .FirstOrDefaultAsync(item => item.TaskItemId == id, cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for status update", id);
                return null;
            }

            task.Status = StatusValidators.ValidateTaskStatus(request.Status);
            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }

        public async Task<PagedResultDto<TaskItemListDto>> GetMyTasksAsync(Guid userId, TaskItemQueryDto query, CancellationToken cancellationToken = default)
        {
            var tasks = _context.TaskItems
                .Where(task => task.AssignedUserId == userId);

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
                _ => tasks.OrderBy(task => task.Status == "Doing" ? 0 : task.Status == "Todo" ? 1 : 2)
                    .ThenBy(task => task.DueDate)
            };

            var (items, totalCount) = await tasks
                .Include(task => task.Project)
                .Include(task => task.AssignedUser)
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

        public async Task<TaskItemDetailDto?> GetMyTaskAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .ThenInclude(project => project.Attachments)
                .Include(item => item.AssignedUser)
                .FirstOrDefaultAsync(item => item.TaskItemId == id && item.AssignedUserId == userId, cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Assigned task {TaskId} was not found for user {UserId}", id, userId);
                return null;
            }

            return task.ToDetailDto();
        }

        public async Task<TaskItemDetailDto?> UpdateMyTaskStatusAsync(Guid userId, Guid id, UpdateTaskStatusDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .ThenInclude(project => project.Attachments)
                .Include(item => item.AssignedUser)
                .FirstOrDefaultAsync(item => item.TaskItemId == id && item.AssignedUserId == userId, cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Assigned task {TaskId} was not found for status update by user {UserId}", id, userId);
                return null;
            }

            task.Status = StatusValidators.ValidateTaskStatus(request.Status);
            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }

        private async Task<Guid?> ValidateAssignedUserAsync(Guid? assignedUserId, CancellationToken cancellationToken)
        {
            if (assignedUserId is null)
            {
                return null;
            }

            var assignedUser = await _context.Users.FirstOrDefaultAsync(user => user.UserId == assignedUserId.Value, cancellationToken);
            if (assignedUser is null)
            {
                throw new ValidationException("Assigned user was not found.");
            }

            if (assignedUser.Role != "Contractor")
            {
                throw new ValidationException("Tasks can only be assigned to Contractor users.");
            }

            if (!assignedUser.IsActive)
            {
                throw new ValidationException("Tasks cannot be assigned to inactive users.");
            }

            return assignedUser.UserId;
        }
    }
}
