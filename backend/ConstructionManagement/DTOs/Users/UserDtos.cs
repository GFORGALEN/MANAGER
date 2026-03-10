using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Entities;

namespace ConstructionManagement.DTOs.Users
{
    /// <summary>
    /// Summary user data returned by user list endpoints.
    /// </summary>
    public class UserListDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Detailed user data returned by user detail endpoints.
    /// </summary>
    public class UserDetailDto
    {
        public Guid UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Request body used to update editable user profile fields.
    /// </summary>
    public class UpdateUserDto
    {
        [Required]
        [MinLength(1)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Request body used to change a user's role.
    /// </summary>
    public class UpdateUserRoleDto
    {
        [Required]
        [MinLength(1)]
        public required string Role { get; set; }
    }

    /// <summary>
    /// Request body used to enable or disable a user.
    /// </summary>
    public class UpdateUserActiveDto
    {
        public bool IsActive { get; set; }
    }

    public static class UserDtoMappings
    {
        public static UserListDto ToListDto(this User user)
        {
            return new UserListDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public static UserDetailDto ToDetailDto(this User user)
        {
            return new UserDetailDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
