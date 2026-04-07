using ConstructionManagement.DTOs.Tasks;

namespace ConstructionManagement.Services
{
    public interface ITaskDraftAiService
    {
        Task<AiTaskDraftSuggestionDto?> GenerateTaskDraftAsync(Guid projectId, AiTaskDraftRequestDto request, CancellationToken cancellationToken = default);
    }
}
