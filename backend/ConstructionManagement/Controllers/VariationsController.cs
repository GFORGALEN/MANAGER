using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Variations;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides endpoints for managing variations under projects.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin,PM")]
    public class VariationsController : ControllerBase
    {
        private readonly IVariationService _variationService;

        public VariationsController(IVariationService variationService)
        {
            _variationService = variationService;
        }

        /// <summary>
        /// Gets all variations that belong to a specific project.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>All variations under the specified project.</returns>
        [HttpGet("api/projects/{projectId:guid}/variations")]
        public async Task<ActionResult<PagedResultDto<VariationListDto>>> GetProjectVariations(Guid projectId, [FromQuery] VariationQueryDto query, CancellationToken cancellationToken)
        {
            var variations = await _variationService.GetProjectVariationsAsync(projectId, query, cancellationToken);
            if (variations is null)
            {
                return NotFound();
            }

            return Ok(variations);
        }

        /// <summary>
        /// Gets a single variation by its unique identifier.
        /// </summary>
        /// <param name="id">The variation ID.</param>
        /// <returns>The matching variation if found.</returns>
        [HttpGet("api/variations/{id:guid}")]
        public async Task<ActionResult<VariationDetailDto>> GetVariation(Guid id, CancellationToken cancellationToken)
        {
            var variation = await _variationService.GetVariationAsync(id, cancellationToken);
            if (variation is null)
            {
                return NotFound();
            }

            return Ok(variation);
        }

        /// <summary>
        /// Creates a new variation under a specific project.
        /// </summary>
        /// <param name="projectId">The project ID that the variation belongs to.</param>
        /// <param name="request">The variation data to create.</param>
        /// <returns>The newly created variation.</returns>
        [HttpPost("api/projects/{projectId:guid}/variations")]
        public async Task<ActionResult<VariationDetailDto>> CreateVariation(Guid projectId, [FromBody] CreateVariationDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var variation = await _variationService.CreateVariationAsync(projectId, request, cancellationToken);
            if (variation is null)
            {
                return NotFound();
            }

            return CreatedAtAction(nameof(GetVariation), new { id = variation.VariationId }, variation);
        }

        /// <summary>
        /// Updates editable fields of an existing variation.
        /// </summary>
        /// <param name="id">The variation ID.</param>
        /// <param name="request">The updated variation data.</param>
        /// <returns>The updated variation.</returns>
        [HttpPut("api/variations/{id:guid}")]
        public async Task<ActionResult<VariationDetailDto>> UpdateVariation(Guid id, [FromBody] UpdateVariationDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var variation = await _variationService.UpdateVariationAsync(id, request, cancellationToken);
            if (variation is null)
            {
                return NotFound();
            }

            return Ok(variation);
        }

        /// <summary>
        /// Updates only the status of a variation.
        /// </summary>
        /// <param name="id">The variation ID.</param>
        /// <param name="request">The new status value. Allowed values: Draft, Submitted, Approved, Rejected, NeedInfo.</param>
        /// <returns>The updated variation.</returns>
        [HttpPatch("api/variations/{id:guid}/status")]
        public async Task<ActionResult<VariationDetailDto>> UpdateVariationStatus(Guid id, [FromBody] UpdateVariationStatusDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var variation = await _variationService.UpdateVariationStatusAsync(id, request, cancellationToken);
            if (variation is null)
            {
                return NotFound();
            }

            return Ok(variation);
        }
    }
}
