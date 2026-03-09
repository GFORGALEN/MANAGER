using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Projects;

namespace ConstructionManagement.Services
{
    public interface IProjectService
    {
        Task<PagedResultDto<ProjectListDto>> GetProjectsAsync(ProjectQueryDto query, CancellationToken cancellationToken = default);

        Task<ProjectDetailDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default);

        Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto request, CancellationToken cancellationToken = default);

        Task<ProjectDetailDto?> UpdateProjectAsync(Guid id, UpdateProjectDto request, CancellationToken cancellationToken = default);

        Task<bool> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
