using ConstructionManagement.DTOs.Auth;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides authentication endpoints for API users.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT bearer token.
        /// </summary>
        /// <param name="request">Login credentials.</param>
        /// <returns>A JWT token and user summary.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.LoginAsync(request, cancellationToken);
            if (result is null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(result);
        }

        /// <summary>
        /// Registers a new contractor account.
        /// </summary>
        /// <param name="request">Registration details for the new contractor.</param>
        /// <returns>A success response when registration is completed.</returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            return Ok(await _authService.RegisterAsync(request, cancellationToken));
        }

        /// <summary>
        /// Gets the currently authenticated user profile.
        /// </summary>
        /// <returns>The current authenticated user profile.</returns>
        [Authorize]
        [HttpGet("me")]
        public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var currentUser = await _authService.GetCurrentUserAsync(userId, cancellationToken);
            if (currentUser is null)
            {
                return Unauthorized();
            }

            return Ok(currentUser);
        }
    }
}
