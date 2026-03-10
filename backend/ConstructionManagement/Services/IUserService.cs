using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Users;

namespace ConstructionManagement.Services
{
    public interface IUserService
    {
        Task<PagedResultDto<UserListDto>> GetUsersAsync(UserQueryDto query, CancellationToken cancellationToken = default);

        Task<UserDetailDto?> GetUserAsync(Guid id, CancellationToken cancellationToken = default);

        Task<UserDetailDto?> UpdateUserAsync(Guid id, UpdateUserDto request, CancellationToken cancellationToken = default);

        Task<UserDetailDto?> UpdateUserRoleAsync(Guid id, UpdateUserRoleDto request, CancellationToken cancellationToken = default);

        Task<UserDetailDto?> UpdateUserActiveAsync(Guid id, UpdateUserActiveDto request, CancellationToken cancellationToken = default);
    }
}
