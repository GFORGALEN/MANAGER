using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Notifications;
using ConstructionManagement.DTOs.Tasks;

namespace ConstructionManagement.Services
{
    public interface ITaskItemService
    {
        Task<PagedResultDto<TaskItemListDto>?> GetProjectTasksAsync(Guid projectId, TaskItemQueryDto query, CancellationToken cancellationToken = default);

        Task<TaskItemDetailDto?> GetTaskAsync(Guid id, CancellationToken cancellationToken = default);

        Task<TaskItemDetailDto?> CreateTaskAsync(Guid projectId, CreateTaskItemDto request, CancellationToken cancellationToken = default);

        Task<TaskItemDetailDto?> UpdateTaskAsync(Guid id, UpdateTaskItemDto request, CancellationToken cancellationToken = default);

        Task<TaskItemDetailDto?> UpdateTaskStatusAsync(Guid id, UpdateTaskStatusDto request, CancellationToken cancellationToken = default);

        Task<PagedResultDto<TaskItemListDto>> GetMyTasksAsync(Guid userId, TaskItemQueryDto query, CancellationToken cancellationToken = default);

        Task<TaskItemDetailDto?> GetMyTaskAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

        Task<TaskItemDetailDto?> UpdateMyTaskStatusAsync(Guid userId, Guid id, UpdateTaskStatusDto request, CancellationToken cancellationToken = default);

        Task<TaskSmsResultDto?> SendTaskSmsAsync(Guid taskId, string? customMessage, CancellationToken cancellationToken = default);
    }
}
