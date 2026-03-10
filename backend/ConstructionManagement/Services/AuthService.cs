using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ConstructionManagement.Configurations;
using ConstructionManagement.Data;
using ConstructionManagement.DTOs.Auth;
using ConstructionManagement.Entities;
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
            var usernameOrEmail = request.Username.Trim();
            var user = await _context.Users.FirstOrDefaultAsync(
                item => item.Username == usernameOrEmail || item.Email == usernameOrEmail,
                cancellationToken);

            if (user is null || !user.IsActive || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            return BuildAuthResponse(user);
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            var email = request.Email.Trim();
            if (await _context.Users.AnyAsync(user => user.Email == email, cancellationToken))
            {
                throw new ValidationException("Email address is already registered.");
            }

            var user = new User
            {
                UserId = Guid.NewGuid(),
                Name = request.Name.Trim(),
                Username = email,
                Email = email,
                PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = "Contractor",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            return new RegisterResponseDto
            {
                Message = "Registration completed successfully."
            };
        }

        public async Task<CurrentUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Where(user => user.UserId == userId)
                .Select(user => new CurrentUserDto
                {
                    Id = user.UserId,
                    Name = user.Name,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = user.Role,
                    IsActive = user.IsActive
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        private AuthResponseDto BuildAuthResponse(User user)
        {
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_jwtOptions.ExpirationMinutes);
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
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
