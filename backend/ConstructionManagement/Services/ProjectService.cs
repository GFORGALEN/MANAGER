using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Projects;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(AppDbContext context, IWebHostEnvironment environment, ILogger<ProjectService> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        public async Task<PagedResultDto<ProjectListDto>> GetProjectsAsync(ProjectQueryDto query, CancellationToken cancellationToken = default)
        {
            var projects = _context.Projects.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                projects = projects.Where(project =>
                    project.Name.Contains(keyword) ||
                    project.Code.Contains(keyword) ||
                    project.Address.Contains(keyword) ||
                    (project.ClientName != null && project.ClientName.Contains(keyword)));
            }

            projects = (query.SortBy?.ToLowerInvariant(), query.SortOrder?.ToLowerInvariant()) switch
            {
                ("name", "desc") => projects.OrderByDescending(project => project.Name),
                ("name", _) => projects.OrderBy(project => project.Name),
                ("createdat", "asc") => projects.OrderBy(project => project.CreatedAt),
                _ => projects.OrderByDescending(project => project.CreatedAt)
            };

            var (items, totalCount) = await projects
                .Select(project => project.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<ProjectListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<ProjectDetailDto?> GetProjectAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var project = await _context.Projects.FindAsync([id], cancellationToken);
            if (project is null)
            {
                _logger.LogWarning("Project {ProjectId} was not found", id);
                return null;
            }

            return project.ToDetailDto();
        }

        public async Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto request, CancellationToken cancellationToken = default)
        {
            var project = new Project
            {
                ProjectId = Guid.NewGuid(),
                Code = request.Code.Trim(),
                Name = request.Name.Trim(),
                Address = request.Address.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                ClientName = string.IsNullOrWhiteSpace(request.ClientName) ? null : request.ClientName.Trim(),
                Status = request.Status.Trim(),
                Budget = request.Budget,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project created with id {ProjectId}", project.ProjectId);
            return project.ToDetailDto();
        }

        public async Task<ProjectDetailDto?> UpdateProjectAsync(Guid id, UpdateProjectDto request, CancellationToken cancellationToken = default)
        {
            var project = await _context.Projects.FindAsync([id], cancellationToken);
            if (project is null)
            {
                _logger.LogWarning("Project {ProjectId} was not found for update", id);
                return null;
            }

            project.Code = request.Code.Trim();
            project.Name = request.Name.Trim();
            project.Address = request.Address.Trim();
            project.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            project.ClientName = string.IsNullOrWhiteSpace(request.ClientName) ? null : request.ClientName.Trim();
            project.Status = request.Status.Trim();
            project.Budget = request.Budget;
            project.StartDate = request.StartDate;
            project.EndDate = request.EndDate;

            await _context.SaveChangesAsync(cancellationToken);
            return project.ToDetailDto();
        }

        public async Task<bool> DeleteProjectAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var project = await _context.Projects.FindAsync([id], cancellationToken);
            if (project is null)
            {
                _logger.LogWarning("Project {ProjectId} was not found for deletion", id);
                return false;
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<int> DeleteAllProjectsAsync(CancellationToken cancellationToken = default)
        {
            var projects = await _context.Projects
                .Include(project => project.Attachments)
                .ToListAsync(cancellationToken);

            var deletedCount = projects.Count;
            if (deletedCount == 0)
            {
                return 0;
            }

            foreach (var attachment in projects.SelectMany(project => project.Attachments))
            {
                if (string.IsNullOrWhiteSpace(attachment.FilePath))
                {
                    continue;
                }

                var fullPath = Path.Combine(_environment.ContentRootPath, attachment.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }

            _context.Projects.RemoveRange(projects);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("All project data was deleted by an administrator. Deleted project count: {DeletedCount}", deletedCount);
            return deletedCount;
        }
    }
}
