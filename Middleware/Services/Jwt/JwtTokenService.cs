using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Middleware.Services.Jwt
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _config;
        public JwtTokenService(IConfiguration config) { _config = config; }

        private SymmetricSecurityKey GetKey() =>
            new(Encoding.UTF8.GetBytes(_config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing.")));
        private string Issuer => _config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        private string AccessAudience => _config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        private string ResetAudience => _config["Jwt:ResetAudience"] ?? throw new InvalidOperationException("Jwt:ResetAudience is missing.");
        private int AccessTokenMinutes =>
            int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) && m > 0 ? m : 30;

        public string CreateAccessToken(int userId, string email, int tokenVersion)
        {
            var token = new JwtSecurityToken(
                issuer: Issuer, audience: AccessAudience,
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Email, email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim("tv", tokenVersion.ToString())
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(AccessTokenMinutes),
                signingCredentials: new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateResetToken(string email, string passwordStamp)
        {
            var token = new JwtSecurityToken(
                issuer: Issuer, audience: ResetAudience,
                claims: new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim("purpose", "reset"),
                    new Claim("pwd", passwordStamp)
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ResetTokenInfo? ValidateResetToken(string token)
        {
            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = Issuer,
                    ValidateAudience = true,
                    ValidAudience = ResetAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = GetKey()
                }, out _);

                if (principal.FindFirst("purpose")?.Value != "reset") return null;
                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                var stamp = principal.FindFirst("pwd")?.Value;
                return email == null || stamp == null ? null : new ResetTokenInfo(email, stamp);
            }
            catch (Exception ex) when (ex is SecurityTokenException || ex is ArgumentException)
            {
                return null;
            }
        }
    }
}