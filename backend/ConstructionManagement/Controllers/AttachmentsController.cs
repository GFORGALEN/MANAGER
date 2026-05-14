using ConstructionManagement.DTOs.Attachments;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.Entities;
using ConstructionManagement.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides endpoints for managing attachment metadata under projects.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin,PM")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentsController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        /// <summary>
        /// Gets all attachments that belong to a specific project.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>All attachments under the specified project.</returns>
        [HttpGet("api/projects/{projectId:guid}/attachments")]
        public async Task<ActionResult<PagedResultDto<AttachmentListDto>>> GetProjectAttachments(Guid projectId, [FromQuery] AttachmentQueryDto query, CancellationToken cancellationToken)
        {
            var attachments = await _attachmentService.GetProjectAttachmentsAsync(projectId, query, cancellationToken);
            if (attachments is null)
            {
                return NotFound();
            }

            return Ok(attachments);
        }

        /// <summary>
        /// Gets a single attachment by its unique identifier.
        /// </summary>
        /// <param name="id">The attachment ID.</param>
        /// <returns>The matching attachment if found.</returns>
        [HttpGet("api/attachments/{id:guid}")]
        public async Task<ActionResult<AttachmentDetailDto>> GetAttachment(Guid id, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentService.GetAttachmentAsync(id, cancellationToken);
            if (attachment is null)
            {
                return NotFound();
            }

            return Ok(attachment);
        }

        /// <summary>
        /// Creates attachment metadata under a specific project.
        /// </summary>
        /// <param name="projectId">The project ID that the attachment belongs to.</param>
        /// <param name="request">The attachment metadata to create.</param>
        /// <returns>The newly created attachment metadata.</returns>
        [HttpPost("api/projects/{projectId:guid}/attachments")]
        public async Task<ActionResult<AttachmentDetailDto>> CreateAttachment(Guid projectId, [FromBody] CreateAttachmentDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var attachment = await _attachmentService.CreateAttachmentAsync(projectId, request, GetCurrentUserId(), cancellationToken);
            if (attachment is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetAttachment), new { id = attachment.AttachmentId }, attachment);
        }

        /// <summary>
        /// Updates editable metadata of an existing attachment.
        /// </summary>
        /// <param name="id">The attachment ID.</param>
        /// <param name="request">The updated attachment metadata.</param>
        /// <returns>The updated attachment.</returns>
        [HttpPut("api/attachments/{id:guid}")]
        public async Task<ActionResult<AttachmentDetailDto>> UpdateAttachment(Guid id, [FromBody] UpdateAttachmentDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var attachment = await _attachmentService.UpdateAttachmentAsync(id, request, GetCurrentUserId(), cancellationToken);
            if (attachment is null)
            {
                return NotFound();
            }

            return Ok(attachment);
        }

        /// <summary>
        /// Uploads a physical file and stores attachment metadata under a project.
        /// </summary>
        /// <param name="projectId">The project ID that the attachment belongs to.</param>
        /// <param name="file">The uploaded file.</param>
        /// <returns>The stored attachment metadata.</returns>
        [HttpPost("api/projects/{projectId:guid}/attachments/upload")]
        public async Task<ActionResult<AttachmentDetailDto>> UploadAttachment(Guid projectId, IFormFile file, CancellationToken cancellationToken)
        {
            var attachment = await _attachmentService.UploadAttachmentAsync(projectId, file, GetCurrentUserId(), cancellationToken);
            if (attachment is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetAttachment), new { id = attachment.AttachmentId }, attachment);
        }

        /// <summary>
        /// Deletes an attachment by its unique identifier.
        /// </summary>
        /// <param name="id">The attachment ID.</param>
        /// <returns>An empty success response if the attachment was deleted.</returns>
        [HttpDelete("api/attachments/{id:guid}")]
        public async Task<IActionResult> DeleteAttachment(Guid id, CancellationToken cancellationToken)
        {
            if (!await _attachmentService.DeleteAttachmentAsync(id, GetCurrentUserId(), cancellationToken))
            {
                return NotFound();
            }

            return Ok();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("Missing user id claim.");
            return Guid.Parse(userIdValue);
        }
    }

}
