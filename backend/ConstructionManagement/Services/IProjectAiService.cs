using ConstructionManagement.DTOs.Projects;

namespace ConstructionManagement.Services
{
    public interface IProjectAiService
    {
        Task<AiWeeklySummaryDto?> GenerateWeeklySummaryAsync(Guid projectId, AiWeeklySummaryRequestDto request, CancellationToken cancellationToken = default);
    }
}
