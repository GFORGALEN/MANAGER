using ConstructionManagement.DTOs.Attachments;
using ConstructionManagement.DTOs.Common;

namespace ConstructionManagement.Services
{
    public interface IAttachmentService
    {
        Task<PagedResultDto<AttachmentListDto>?> GetProjectAttachmentsAsync(Guid projectId, AttachmentQueryDto query, CancellationToken cancellationToken = default);

        Task<AttachmentDetailDto?> GetAttachmentAsync(Guid id, CancellationToken cancellationToken = default);

        Task<AttachmentDetailDto?> CreateAttachmentAsync(Guid projectId, CreateAttachmentDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default);

        Task<AttachmentDetailDto?> UpdateAttachmentAsync(Guid id, UpdateAttachmentDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default);

        Task<AttachmentDetailDto?> UploadAttachmentAsync(Guid projectId, IFormFile file, Guid? actorUserId = null, CancellationToken cancellationToken = default);

        Task<bool> DeleteAttachmentAsync(Guid id, Guid? actorUserId = null, CancellationToken cancellationToken = default);
    }
}
