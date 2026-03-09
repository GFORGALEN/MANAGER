using ConstructionManagement.DTOs.Auth;

namespace ConstructionManagement.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    }
}
