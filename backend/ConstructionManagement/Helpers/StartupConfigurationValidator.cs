using ConstructionManagement.Configurations;

namespace ConstructionManagement.Helpers
{
    public static class StartupConfigurationValidator
    {
        private static readonly string[] DisallowedJwtSecrets =
        [
            "",
            "replace_with_a_long_random_secret",
            "replace-with-a-long-random-jwt-secret",
            "super-secret-key-for-construction-management-api-2026"
        ];

        public static void ValidateForCurrentEnvironment(
            IWebHostEnvironment environment,
            JwtOptions jwtOptions,
            FrontendOptions frontendOptions,
            BootstrapAdminOptions bootstrapAdminOptions)
        {
            if (environment.IsDevelopment())
            {
                return;
            }

            if (DisallowedJwtSecrets.Contains(jwtOptions.SecretKey, StringComparer.Ordinal) || jwtOptions.SecretKey.Length < 32)
            {
                throw new InvalidOperationException("Production startup blocked: configure a strong Jwt:SecretKey before launch.");
            }

            var allowedOrigins = frontendOptions.AllowedOrigins
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Select(origin => origin.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (!allowedOrigins.Any(IsNonLocalOrigin))
            {
                throw new InvalidOperationException("Production startup blocked: configure Frontend:AllowedOrigins with at least one real frontend domain.");
            }

            if (!bootstrapAdminOptions.Enabled)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(bootstrapAdminOptions.Name) ||
                string.IsNullOrWhiteSpace(bootstrapAdminOptions.Username) ||
                string.IsNullOrWhiteSpace(bootstrapAdminOptions.Email) ||
                string.IsNullOrWhiteSpace(bootstrapAdminOptions.Password))
            {
                throw new InvalidOperationException("Production startup blocked: BootstrapAdmin is enabled but required fields are missing.");
            }
        }

        private static bool IsNonLocalOrigin(string origin)
        {
            if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            {
                return false;
            }

            return !uri.IsLoopback &&
                   !string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) &&
                   !string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
