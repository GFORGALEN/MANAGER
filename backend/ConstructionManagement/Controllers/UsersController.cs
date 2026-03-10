using ConstructionManagement.DTOs.Common;
using ConstructionManagement.DTOs.Users;
using ConstructionManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConstructionManagement.Controllers
{
    /// <summary>
    /// Provides user management endpoints for administrative roles.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Gets users for management and assignment workflows.
        /// </summary>
        /// <param name="query">Optional filters and paging settings.</param>
        /// <returns>A paged list of users.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<UserListDto>>> GetUsers([FromQuery] UserQueryDto query, CancellationToken cancellationToken)
        {
            return Ok(await _userService.GetUsersAsync(query, cancellationToken));
        }

        /// <summary>
        /// Gets a single user by identifier.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <returns>The matching user if found.</returns>
        [Authorize(Roles = "Admin,PM")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDetailDto>> GetUser(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userService.GetUserAsync(id, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        /// <summary>
        /// Updates editable user profile fields.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="request">The updated user fields.</param>
        /// <returns>The updated user if found.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserDetailDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userService.UpdateUserAsync(id, request, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        /// <summary>
        /// Changes a user's role.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="request">The new role value.</param>
        /// <returns>The updated user if found.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/role")]
        public async Task<ActionResult<UserDetailDto>> UpdateUserRole(Guid id, [FromBody] UpdateUserRoleDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var user = await _userService.UpdateUserRoleAsync(id, request, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        /// <summary>
        /// Enables or disables a user account.
        /// </summary>
        /// <param name="id">The user identifier.</param>
        /// <param name="request">The activation value to apply.</param>
        /// <returns>The updated user if found.</returns>
        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/active")]
        public async Task<ActionResult<UserDetailDto>> UpdateUserActive(Guid id, [FromBody] UpdateUserActiveDto request, CancellationToken cancellationToken)
        {
            var user = await _userService.UpdateUserActiveAsync(id, request, cancellationToken);
            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
    }
}
