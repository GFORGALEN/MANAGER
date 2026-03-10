using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Users;
using ConstructionManagement.Entities;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ConstructionManagement.Services
{
    public class UserService : IUserService
    {
        private static readonly HashSet<string> AllowedRoles = ["Admin", "PM", "Contractor"];

        private readonly AppDbContext _context;
        private readonly ILogger<UserService> _logger;

        public UserService(AppDbContext context, ILogger<UserService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResultDto<UserListDto>> GetUsersAsync(UserQueryDto query, CancellationToken cancellationToken = default)
        {
            var users = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Keyword))
            {
                var keyword = query.Keyword.Trim();
                users = users.Where(user =>
                    user.Name.Contains(keyword) ||
                    user.Email.Contains(keyword) ||
                    user.Username.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(query.Role))
            {
                users = users.Where(user => user.Role == query.Role);
            }

            if (query.IsActive.HasValue)
            {
                users = users.Where(user => user.IsActive == query.IsActive.Value);
            }

            users = users
                .OrderBy(user => user.Role)
                .ThenBy(user => user.Name);

            var (items, totalCount) = await users
                .Select(user => user.ToListDto())
                .ToPagedResultAsync(query.PageNumber, query.PageSize, cancellationToken);

            return new PagedResultDto<UserListDto>
            {
                Items = items,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<UserDetailDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} was not found", id);
                return null;
            }

            return user.ToDetailDto();
        }

        public async Task<UserDetailDto?> UpdateUserAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} was not found for update", id);
                return null;
            }

            var email = request.Email.Trim();
            var emailInUse = await _context.Users.AnyAsync(
                item => item.UserId != id && item.Email == email,
                cancellationToken);

            if (emailInUse)
            {
                throw new ValidationException("Email address is already in use.");
            }

            user.Name = request.Name.Trim();
            user.Email = email;
            user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

            await _context.SaveChangesAsync(cancellationToken);
            return user.ToDetailDto();
        }

        public async Task<UserDetailDto?> UpdateUserRoleAsync(Guid id, UpdateUserRoleDto request, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} was not found for role update", id);
                return null;
            }

            var role = request.Role.Trim();
            if (!AllowedRoles.Contains(role))
            {
                throw new ValidationException("Role must be one of: Admin, PM, Contractor.");
            }

            user.Role = role;
            await _context.SaveChangesAsync(cancellationToken);
            return user.ToDetailDto();
        }

        public async Task<UserDetailDto?> UpdateUserActiveAsync(Guid id, UpdateUserActiveDto request, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync([id], cancellationToken);
            if (user is null)
            {
                _logger.LogWarning("User {UserId} was not found for active update", id);
                return null;
            }

            user.IsActive = request.IsActive;
            await _context.SaveChangesAsync(cancellationToken);
            return user.ToDetailDto();
        }
    }
}
