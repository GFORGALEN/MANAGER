using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Projects;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides CRUD endpoints for managing construction projects.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Admin,PM")]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        /// <summary>
        /// Gets all projects in the system.
        /// </summary>
        /// <returns>A list of all projects.</returns>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProjectListDto>>> GetProjects([FromQuery] ProjectQueryDto query, CancellationToken cancellationToken)
        {
            return Ok(await _projectService.GetProjectsAsync(query, cancellationToken));
        }

        /// <summary>
        /// Gets a single project by its unique identifier.
        /// </summary>
        /// <param name="id">The project ID.</param>
        /// <returns>The matching project if found.</returns>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProjectDetailDto>> GetProject(Guid id, CancellationToken cancellationToken)
        {
            var project = await _projectService.GetProjectAsync(id, cancellationToken);
            if (project is null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        /// <summary>
        /// Creates a new project.
        /// </summary>
        /// <param name="request">The project data to create.</param>
        /// <returns>The newly created project.</returns>
        [HttpPost]
        public async Task<ActionResult<ProjectDetailDto>> CreateProject([FromBody] CreateProjectDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var project = await _projectService.CreateProjectAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetProject), new { id = project.ProjectId }, project);
        }

        /// <summary>
        /// Updates editable fields of an existing project.
        /// </summary>
        /// <param name="id">The project ID.</param>
        /// <param name="request">The updated project data.</param>
        /// <returns>The updated project.</returns>
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ProjectDetailDto>> UpdateProject(Guid id, [FromBody] UpdateProjectDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var project = await _projectService.UpdateProjectAsync(id, request, cancellationToken);
            if (project is null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        /// <summary>
        /// Deletes a project by its unique identifier.
        /// </summary>
        /// <param name="id">The project ID.</param>
        /// <returns>An empty success response if the project was deleted.</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
        {
            if (!await _projectService.DeleteProjectAsync(id, cancellationToken))
            {
                return NotFound();
            }

            return Ok();
        }
    }

}
