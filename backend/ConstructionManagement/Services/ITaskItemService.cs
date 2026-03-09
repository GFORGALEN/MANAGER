using ConstructionManagement.DTOs.Common;
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
    }
}
