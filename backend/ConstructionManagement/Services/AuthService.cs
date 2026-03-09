using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConstructionManagement.Configurations;
using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Auth;
using ConstructionManagement.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConstructionManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly JwtOptions _jwtOptions;

        public AuthService(AppDbContext context, IPasswordHasher passwordHasher, IOptions<JwtOptions> jwtOptions)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var username = request.Username.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(item => item.Username == username, cancellationToken);

            if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_jwtOptions.ExpirationMinutes);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Username = user.Username,
                Role = user.Role,
                ExpiresAtUtc = expires
            };
        }
    }
}
