using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NotesBackend.Models;

namespace NotesBackend.Services
{
    /// <summary>
    /// JWT token generation service using environment variables.
    /// </summary>
    public class JwtTokenService : ITokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _secret;
        private readonly TimeSpan _defaultLifetime;

        public const string EnvJwtIssuer = "JWT_ISSUER";
        public const string EnvJwtAudience = "JWT_AUDIENCE";
        public const string EnvJwtSecret = "JWT_SECRET";
        public const string EnvJwtLifetimeMinutes = "JWT_LIFETIME_MINUTES";

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _issuer = Environment.GetEnvironmentVariable(EnvJwtIssuer) ?? configuration["Jwt:Issuer"] ?? "notes-backend";
            _audience = Environment.GetEnvironmentVariable(EnvJwtAudience) ?? configuration["Jwt:Audience"] ?? "notes-clients";
            _secret = Environment.GetEnvironmentVariable(EnvJwtSecret) ?? configuration["Jwt:Secret"] ?? "CHANGE_ME_DEV_SECRET_32CHARS_MINIMUM";
            var lifetimeStr = Environment.GetEnvironmentVariable(EnvJwtLifetimeMinutes) ?? configuration["Jwt:LifetimeMinutes"] ?? "120";
            if (!int.TryParse(lifetimeStr, out var minutes))
            {
                minutes = 120;
                logger.LogWarning("Invalid JWT_LIFETIME_MINUTES, defaulting to 120");
            }
            _defaultLifetime = TimeSpan.FromMinutes(minutes);
        }

        public TimeSpan GetDefaultLifetime() => _defaultLifetime;

        public string CreateToken(User user, DateTime expiresAtUtc)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAtUtc,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Builds TokenValidationParameters to validate incoming JWTs based on current config.
        /// </summary>
        public TokenValidationParameters BuildValidationParameters()
        {
            return new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        }
    }
}
