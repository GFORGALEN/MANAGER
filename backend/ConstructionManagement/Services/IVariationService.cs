using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Variations;

namespace ConstructionManagement.Services
{
    public interface IVariationService
    {
        Task<PagedResultDto<VariationListDto>?> GetProjectVariationsAsync(Guid projectId, VariationQueryDto query, CancellationToken cancellationToken = default);

        Task<VariationDetailDto?> GetVariationAsync(Guid id, CancellationToken cancellationToken = default);

        Task<VariationDetailDto?> CreateVariationAsync(Guid projectId, CreateVariationDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default);

        Task<VariationDetailDto?> UpdateVariationAsync(Guid id, UpdateVariationDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default);

        Task<VariationDetailDto?> UpdateVariationStatusAsync(Guid id, UpdateVariationStatusDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default);
    }
}
