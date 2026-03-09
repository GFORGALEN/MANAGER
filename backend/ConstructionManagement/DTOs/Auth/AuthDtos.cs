using System.ComponentModel.DataAnnotations;

namespace ConstructionManagement.DTOs.Auth
{
    public class LoginRequestDto
    {
        [Required]
        [MinLength(1)]
        public required string Username { get; set; }

        [Required]
        [MinLength(1)]
        public required string Password { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime ExpiresAtUtc { get; set; }
    }
}
