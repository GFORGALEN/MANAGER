using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Attachments;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class AttachmentService : IAttachmentService
    {
        private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg", ".doc", ".docx", ".xlsx"];

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AttachmentService> _logger;
        private readonly IAuditLogService _auditLogService;

        public AttachmentService(AppDbContext context, IWebHostEnvironment environment, ILogger<AttachmentService> logger, IAuditLogService auditLogService)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
            _auditLogService = auditLogService;
        }

        public async Task<PagedResultDto<AttachmentListDto>?> GetProjectAttachmentsAsync(Guid projectId, AttachmentQueryDto query, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when querying attachments", projectId);
                return null;
            }

            var attachments = _context.Attachments.Where(attachment => attachment.ProjectId == projectId);
            attachments = (query.SortBy?.ToLowerInvariant(), query.SortOrder?.ToLowerInvariant()) switch
            {
                ("filename", "desc") => attachments.OrderByDescending(attachment => attachment.FileName),
                ("filename", _) => attachments.OrderBy(attachment => attachment.FileName),
                _ => attachments.OrderByDescending(attachment => attachment.UploadedAt)
            };

            var (items, totalCount) = await attachments
                .Select(attachment => attachment.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<AttachmentListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<AttachmentDetailDto?> GetAttachmentAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var attachment = await _context.Attachments.FindAsync([id], cancellationToken);
            if (attachment is null)
            {
                _logger.LogWarning("Attachment {AttachmentId} was not found", id);
                return null;
            }

            return attachment.ToDetailDto();
        }

        public async Task<AttachmentDetailDto?> CreateAttachmentAsync(Guid projectId, CreateAttachmentDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when creating attachment", projectId);
                return null;
            }

            var attachment = new Attachment
            {
                AttachmentId = Guid.NewGuid(),
                ProjectId = projectId,
                FileName = request.FileName.Trim(),
                FilePath = request.FilePath.Trim(),
                ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? null : request.ContentType.Trim(),
                FileSize = 0,
                UploadedAt = DateTime.UtcNow
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Attachment", attachment.AttachmentId, "Created", null, null, attachment.FileName, $"Attachment metadata created for project {projectId}", cancellationToken);
            _logger.LogInformation("Attachment metadata {AttachmentId} created for project {ProjectId}", attachment.AttachmentId, projectId);
            return attachment.ToDetailDto();
        }

        public async Task<AttachmentDetailDto?> UpdateAttachmentAsync(Guid id, UpdateAttachmentDto request, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            var attachment = await _context.Attachments.FindAsync([id], cancellationToken);
            if (attachment is null)
            {
                _logger.LogWarning("Attachment {AttachmentId} was not found for update", id);
                return null;
            }

            attachment.FileName = request.FileName.Trim();
            attachment.FilePath = request.FilePath.Trim();
            attachment.ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? null : request.ContentType.Trim();

            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Attachment", attachment.AttachmentId, "Updated", null, null, attachment.FileName, "Attachment metadata updated", cancellationToken);
            return attachment.ToDetailDto();
        }

        public async Task<AttachmentDetailDto?> UploadAttachmentAsync(Guid projectId, IFormFile file, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            if (!await _context.Projects.AnyAsync(project => project.ProjectId == projectId, cancellationToken))
            {
                _logger.LogWarning("Project {ProjectId} was not found when uploading attachment", projectId);
                return null;
            }

            if (file.Length == 0)
            {
                throw new ValidationException("Uploaded file must not be empty.");
            }

            if (file.Length > 10 * 1024 * 1024)
            {
                throw new ValidationException("Uploaded file exceeds the 10 MB size limit.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                throw new ValidationException("Uploaded file type is not allowed.");
            }

            var uploadsRoot = Path.Combine(_environment.ContentRootPath, "Uploads");
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var storedPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var stream = File.Create(storedPath))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relativePath = Path.Combine("Uploads", storedFileName).Replace("\\", "/");
            var attachment = new Attachment
            {
                AttachmentId = Guid.NewGuid(),
                ProjectId = projectId,
                FileName = Path.GetFileName(file.FileName),
                FilePath = relativePath,
                ContentType = file.ContentType,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow
            };

            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Attachment", attachment.AttachmentId, "Uploaded", null, null, attachment.FileName, $"Attachment uploaded for project {projectId}", cancellationToken);
            _logger.LogInformation("Attachment {AttachmentId} uploaded for project {ProjectId}", attachment.AttachmentId, projectId);
            return attachment.ToDetailDto();
        }

        public async Task<bool> DeleteAttachmentAsync(Guid id, Guid? actorUserId = null, CancellationToken cancellationToken = default)
        {
            var attachment = await _context.Attachments.FindAsync([id], cancellationToken);
            if (attachment is null)
            {
                _logger.LogWarning("Attachment {AttachmentId} was not found for deletion", id);
                return false;
            }

            var fullPath = Path.Combine(_environment.ContentRootPath, attachment.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            var fileName = attachment.FileName;
            _context.Attachments.Remove(attachment);
            await _context.SaveChangesAsync(cancellationToken);
            await _auditLogService.RecordAsync(actorUserId, "Attachment", id, "Deleted", null, fileName, null, "Attachment deleted", cancellationToken);
            _logger.LogInformation("Attachment {AttachmentId} deleted", id);
            return true;
        }
    }
}
