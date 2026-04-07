using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Notifications;
using ConstructionManagement.DTOs.Tasks;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class TaskItemService : ITaskItemService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TaskItemService> _logger;
        private readonly ISmsService _smsService;
        private readonly IEmailService _emailService;

        public TaskItemService(AppDbContext context, ILogger<TaskItemService> logger, ISmsService smsService, IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _smsService = smsService;
            _emailService = emailService;
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
                .Include(task => task.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
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
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
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
                Status = "Draft",
                Priority = NormalizePriority(request.Priority),
                Category = NormalizeCategory(request.Category),
                EstimatedHours = NormalizeEstimatedHours(request.EstimatedHours),
                StartDate = NormalizeStartDate(request.StartDate),
                DueDate = request.DueDate,
                AssignedUserId = await ValidateAssignedUserAsync(request.AssignedUserId, cancellationToken),
                CreatedAt = DateTime.UtcNow
            };

            var assignmentIds = await ValidateAssignedUsersAsync(request.AssignedUserId, request.AssignedUserIds, cancellationToken);
            task.AssignedUserId = assignmentIds.FirstOrDefault();
            task.TaskAssignments = assignmentIds
                .Select(userId => new TaskAssignment
                {
                    TaskItemId = task.TaskItemId,
                    UserId = userId
                })
                .ToList();

            if (task.DueDate < task.StartDate)
            {
                throw new ValidationException("Task due date cannot be earlier than start date.");
            }

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
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
                .FirstOrDefaultAsync(item => item.TaskItemId == id, cancellationToken);
            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for update", id);
                return null;
            }

            task.Title = request.Title.Trim();
            task.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            task.Priority = NormalizePriority(request.Priority);
            task.Category = NormalizeCategory(request.Category);
            task.EstimatedHours = NormalizeEstimatedHours(request.EstimatedHours);
            task.StartDate = NormalizeStartDate(request.StartDate, task.StartDate);
            task.DueDate = request.DueDate;
            var assignmentIds = await ValidateAssignedUsersAsync(request.AssignedUserId, request.AssignedUserIds, cancellationToken);
            task.AssignedUserId = assignmentIds.FirstOrDefault();
            await SyncTaskAssignmentsAsync(task, assignmentIds, cancellationToken);

            if (task.DueDate < task.StartDate)
            {
                throw new ValidationException("Task due date cannot be earlier than start date.");
            }

            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }

        public async Task<TaskItemDetailDto?> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .ThenInclude(project => project.Attachments)
                .Include(item => item.AssignedUser)
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
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
                .Where(task => task.TaskAssignments.Any(taskAssignment => taskAssignment.UserId == userId));

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
                _ => tasks.OrderBy(task =>
                        task.Status == "InProgress" ? 0 :
                        task.Status == "Blocked" ? 1 :
                        task.Status == "Draft" ? 2 : 3)
                    .ThenBy(task => task.DueDate)
            };

            var (items, totalCount) = await tasks
                .Include(task => task.Project)
                .Include(task => task.AssignedUser)
                .Include(task => task.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
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
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
                .FirstOrDefaultAsync(item => item.TaskItemId == id && item.TaskAssignments.Any(taskAssignment => taskAssignment.UserId == userId), cancellationToken);

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
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
                .FirstOrDefaultAsync(item => item.TaskItemId == id && item.TaskAssignments.Any(taskAssignment => taskAssignment.UserId == userId), cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Assigned task {TaskId} was not found for status update by user {UserId}", id, userId);
                return null;
            }

            task.Status = StatusValidators.ValidateTaskStatus(request.Status);
            await _context.SaveChangesAsync(cancellationToken);
            return task.ToDetailDto();
        }

        public async Task<TaskAdminNotificationResultDto?> NotifyMyTaskAdminAsync(Guid userId, Guid id, NotifyTaskAdminDto request, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
                .FirstOrDefaultAsync(item => item.TaskItemId == id && item.TaskAssignments.Any(taskAssignment => taskAssignment.UserId == userId), cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Assigned task {TaskId} was not found for admin notification by user {UserId}", id, userId);
                return null;
            }

            var worker = await _context.Users.FirstOrDefaultAsync(user => user.UserId == userId, cancellationToken);
            if (worker is null)
            {
                throw new ValidationException("Worker account was not found.");
            }

            var topic = NormalizeAdminNotificationTopic(request.Topic);
            var messageBody = request.Message.Trim();
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                throw new ValidationException("Message is required.");
            }

            var recipients = await _context.Users
                .Where(user =>
                    user.IsActive &&
                    (user.Role == "Admin" || user.Role == "PM") &&
                    !string.IsNullOrWhiteSpace(user.Email))
                .ToListAsync(cancellationToken);

            if (recipients.Count == 0)
            {
                throw new ValidationException("No active admin or PM recipients are available.");
            }

            var subjectPrefix = topic == "Issue" ? "Worker Issue Report" : "Worker Completion Report";
            var subject = $"{subjectPrefix}: {task.Title}";
            var body =
                $"Topic: {topic}{Environment.NewLine}" +
                $"Worker: {worker.Name} <{worker.Email}>{Environment.NewLine}" +
                $"Project: {task.Project?.Name ?? "Project"}{Environment.NewLine}" +
                $"Task: {task.Title}{Environment.NewLine}" +
                $"Status: {task.Status}{Environment.NewLine}" +
                $"Start: {task.StartDate:yyyy-MM-dd HH:mm}{Environment.NewLine}" +
                $"Due: {task.DueDate:yyyy-MM-dd HH:mm}{Environment.NewLine}{Environment.NewLine}" +
                $"Message:{Environment.NewLine}{messageBody}";

            var result = new TaskAdminNotificationResultDto
            {
                TaskItemId = task.TaskItemId,
                Topic = topic
            };

            foreach (var recipient in recipients)
            {
                result.AttemptedCount++;

                try
                {
                    await _emailService.SendAsync(recipient.Email.Trim(), subject, body, cancellationToken);
                    result.SentCount++;
                    result.SentRecipients.Add($"{recipient.Name} <{recipient.Email.Trim()}>");
                }
                catch (ValidationException ex)
                {
                    result.FailedRecipients.Add($"{recipient.Name} ({ex.Message})");
                }
                catch (SmtpException ex)
                {
                    result.FailedRecipients.Add($"{recipient.Name} ({ex.Message})");
                }
            }

            _logger.LogInformation(
                "Worker task admin notification completed for task {TaskId}. Topic: {Topic}. Attempted: {AttemptedCount}, Sent: {SentCount}",
                task.TaskItemId,
                topic,
                result.AttemptedCount,
                result.SentCount);

            return result;
        }

        public async Task<TaskSmsResultDto?> SendTaskSmsAsync(Guid taskId, string? customMessage, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
                .FirstOrDefaultAsync(item => item.TaskItemId == taskId, cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for SMS notification", taskId);
                return null;
            }

            var recipients = task.TaskAssignments
                .Select(taskAssignment => taskAssignment.User)
                .Where(user => user.IsActive)
                .GroupBy(user => user.UserId)
                .Select(group => group.First())
                .ToList();

            if (recipients.Count == 0)
            {
                throw new ValidationException("This task has no active assigned users to notify.");
            }

            var messageBody = BuildTaskSmsMessage(task, customMessage);
            var result = new TaskSmsResultDto
            {
                TaskItemId = task.TaskItemId,
                TaskTitle = task.Title
            };

            foreach (var user in recipients)
            {
                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    result.SkippedRecipients.Add($"{user.Name} (missing phone number)");
                    continue;
                }

                if (!LooksLikeE164(user.PhoneNumber))
                {
                    result.SkippedRecipients.Add($"{user.Name} (phone must be in E.164 format)");
                    continue;
                }

                result.AttemptedCount++;

                try
                {
                    await _smsService.SendAsync(user.PhoneNumber.Trim(), messageBody, cancellationToken);
                    result.SentCount++;
                }
                catch (ValidationException ex)
                {
                    result.FailedRecipients.Add($"{user.Name} ({ex.Message})");
                }
            }

            _logger.LogInformation(
                "Task SMS notification completed for task {TaskId}. Attempted: {AttemptedCount}, Sent: {SentCount}",
                taskId,
                result.AttemptedCount,
                result.SentCount);

            return result;
        }

        public async Task<TaskEmailResultDto?> SendTaskEmailAsync(Guid taskId, string? customMessage, CancellationToken cancellationToken = default)
        {
            var task = await _context.TaskItems
                .Include(item => item.Project)
                .Include(item => item.TaskAssignments)
                .ThenInclude(taskAssignment => taskAssignment.User)
                .FirstOrDefaultAsync(item => item.TaskItemId == taskId, cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Task {TaskId} was not found for email notification", taskId);
                return null;
            }

            var recipients = task.TaskAssignments
                .Select(taskAssignment => taskAssignment.User)
                .Where(user => user.IsActive)
                .GroupBy(user => user.UserId)
                .Select(group => group.First())
                .ToList();

            if (recipients.Count == 0)
            {
                throw new ValidationException("This task has no active assigned users to notify.");
            }

            var subject = $"Task Update: {task.Title}";
            var emailBody = BuildTaskEmailMessage(task, customMessage);
            var result = new TaskEmailResultDto
            {
                TaskItemId = task.TaskItemId,
                TaskTitle = task.Title
            };

            foreach (var user in recipients)
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    result.SkippedRecipients.Add($"{user.Name} (missing email address)");
                    continue;
                }

                result.AttemptedCount++;

                try
                {
                    await _emailService.SendAsync(user.Email.Trim(), subject, emailBody, cancellationToken);
                    result.SentCount++;
                    result.SentRecipients.Add($"{user.Name} <{user.Email.Trim()}>");
                }
                catch (ValidationException ex)
                {
                    result.FailedRecipients.Add($"{user.Name} ({ex.Message})");
                }
                catch (SmtpException ex)
                {
                    result.FailedRecipients.Add($"{user.Name} ({ex.Message})");
                }
            }

            _logger.LogInformation(
                "Task email notification completed for task {TaskId}. Attempted: {AttemptedCount}, Sent: {SentCount}",
                taskId,
                result.AttemptedCount,
                result.SentCount);

            return result;
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

            if (!assignedUser.IsActive)
            {
                throw new ValidationException("Tasks cannot be assigned to inactive users.");
            }

            return assignedUser.UserId;
        }

        private async Task<List<Guid>> ValidateAssignedUsersAsync(Guid? assignedUserId, List<Guid>? assignedUserIds, CancellationToken cancellationToken)
        {
            var normalizedIds = (assignedUserIds ?? [])
                .Append(assignedUserId ?? Guid.Empty)
                .Where(userId => userId != Guid.Empty)
                .Distinct()
                .ToList();

            if (normalizedIds.Count == 0)
            {
                return [];
            }

            var users = await _context.Users
                .Where(user => normalizedIds.Contains(user.UserId))
                .ToListAsync(cancellationToken);

            if (users.Count != normalizedIds.Count)
            {
                throw new ValidationException("One or more assigned users were not found.");
            }

            if (users.Any(user => !user.IsActive))
            {
                throw new ValidationException("Tasks cannot be assigned to inactive users.");
            }

            return normalizedIds;
        }

        private async Task SyncTaskAssignmentsAsync(TaskItem task, List<Guid> assignmentIds, CancellationToken cancellationToken)
        {
            await _context.Entry(task)
                .Collection(item => item.TaskAssignments)
                .LoadAsync(cancellationToken);

            var existingAssignments = task.TaskAssignments.ToList();
            var toRemove = existingAssignments
                .Where(taskAssignment => !assignmentIds.Contains(taskAssignment.UserId))
                .ToList();

            if (toRemove.Count > 0)
            {
                _context.TaskAssignments.RemoveRange(toRemove);
            }

            var existingUserIds = existingAssignments
                .Select(taskAssignment => taskAssignment.UserId)
                .ToHashSet();

            foreach (var userId in assignmentIds.Where(userId => !existingUserIds.Contains(userId)))
            {
                task.TaskAssignments.Add(new TaskAssignment
                {
                    TaskItemId = task.TaskItemId,
                    UserId = userId
                });
            }
        }

        private static DateTime NormalizeStartDate(DateTime? startDate, DateTime? fallback = null)
        {
            return startDate ?? fallback ?? DateTime.UtcNow;
        }

        private static string NormalizePriority(string? value)
        {
            return value?.Trim() switch
            {
                "Low" => "Low",
                "Medium" => "Medium",
                "High" => "High",
                "Critical" => "Critical",
                _ => "Medium"
            };
        }

        private static string? NormalizeCategory(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static decimal? NormalizeEstimatedHours(decimal? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return Math.Round(Math.Max(0.5m, value.Value), 1, MidpointRounding.AwayFromZero);
        }

        private static string BuildTaskSmsMessage(TaskItem task, string? customMessage)
        {
            if (!string.IsNullOrWhiteSpace(customMessage))
            {
                return customMessage.Trim();
            }

            var projectName = task.Project?.Name ?? "Project";
            return $"Task update: {task.Title}. Project: {projectName}. Status: {task.Status}. Start: {task.StartDate:yyyy-MM-dd HH:mm}. Due: {task.DueDate:yyyy-MM-dd HH:mm}.";
        }

        private static bool LooksLikeE164(string phoneNumber)
        {
            var normalized = phoneNumber.Trim();
            return normalized.Length >= 8 &&
                normalized.StartsWith('+') &&
                normalized.Skip(1).All(char.IsDigit);
        }

        private static string BuildTaskEmailMessage(TaskItem task, string? customMessage)
        {
            if (!string.IsNullOrWhiteSpace(customMessage))
            {
                return customMessage.Trim();
            }

            var projectName = task.Project?.Name ?? "Project";
            return
                $"Task update{Environment.NewLine}" +
                $"Project: {projectName}{Environment.NewLine}" +
                $"Task: {task.Title}{Environment.NewLine}" +
                $"Status: {task.Status}{Environment.NewLine}" +
                $"Start: {task.StartDate:yyyy-MM-dd HH:mm}{Environment.NewLine}" +
                $"Due: {task.DueDate:yyyy-MM-dd HH:mm}";
        }

        private static string NormalizeAdminNotificationTopic(string topic)
        {
            return topic.Trim().ToLowerInvariant() switch
            {
                "issue" => "Issue",
                "completion" => "Completion",
                _ => throw new ValidationException("Topic must be either Issue or Completion.")
            };
        }
    }
}
