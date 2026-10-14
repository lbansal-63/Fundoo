using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Helpers;
using BusinessLayer.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ModelLayer.DTOs;
using ModelLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class UserBusiness : IUserBusiness
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // "Unknown email" aur "wrong password" ka time lagbhag same rakhne ke liye
        private static readonly string DummyHash =
            BCrypt.Net.BCrypt.HashPassword("fundoo-dummy-password-for-timing");

        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;
        private readonly EmailHelper _email;
        private readonly ILogger<UserBusiness> _logger;

        public UserBusiness(
            IUserRepository repo,
            IConfiguration config,
            EmailHelper email,
            ILogger<UserBusiness> logger)
        {
            _repo = repo;
            _config = config;
            _email = email;
            _logger = logger;
        }

        // ---------- config helpers ----------

        private SymmetricSecurityKey GetKey()
        {
            var keyConfig = _config["Jwt:Key"]
                ?? throw new InvalidOperationException("Jwt:Key is missing. Set it with user-secrets or an environment variable.");
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyConfig));
        }

        private string Issuer => _config["Jwt:Issuer"]
            ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

        private string AccessAudience => _config["Jwt:Audience"]
            ?? throw new InvalidOperationException("Jwt:Audience is missing.");

        // Alag audience => reset token kabhi login token ki tarah accept nahi hoga
        private string ResetAudience => _config["Jwt:ResetAudience"]
            ?? throw new InvalidOperationException("Jwt:ResetAudience is missing.");

        private int AccessTokenMinutes =>
            int.TryParse(_config["Jwt:AccessTokenMinutes"], out var m) && m > 0 ? m : 30;

        // Password badalte hi ye badal jata hai, isliye reset token sirf ek baar chalega
        private static string PasswordStamp(string passwordHash) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)))[..16];

        // ---------- register ----------

        public bool Register(RegisterDto dto)
        {
            if (_repo.GetByEmail(dto.Email) != null)
                return false;

            _repo.Add(new UserEntity
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            });

            _logger.LogInformation("AUDIT: new user registered");
            return true;
        }

        // ---------- login ----------

        public string? Login(LoginDto dto)
        {
            var user = _repo.GetByEmail(dto.Email);

            if (user == null)
            {
                BCrypt.Net.BCrypt.Verify(dto.Password, DummyHash); // timing similar rakhne ke liye
                _logger.LogWarning("AUDIT: login failed - unknown email");
                return null;
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("AUDIT: login blocked - user {UserId} is locked out", user.Id);
                return null; // wrong password jaisa hi generic failure
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                user.FailedLoginCount++;
                if (user.FailedLoginCount >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.Add(LockoutDuration);
                    user.FailedLoginCount = 0;
                    _logger.LogWarning("AUDIT: user {UserId} locked out after too many failed logins", user.Id);
                }
                else
                {
                    _logger.LogWarning("AUDIT: login failed for user {UserId} (attempt {Count})",
                        user.Id, user.FailedLoginCount);
                }
                _repo.Update(user);
                return null;
            }

            if (user.FailedLoginCount != 0 || user.LockoutEnd != null)
            {
                user.FailedLoginCount = 0;
                user.LockoutEnd = null;
                _repo.Update(user);
            }

            _logger.LogInformation("AUDIT: login success for user {UserId}", user.Id);
            return CreateAccessToken(user);
        }

        private string CreateAccessToken(UserEntity user)
        {
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: AccessAudience,
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(AccessTokenMinutes),
                signingCredentials: new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ---------- forgot / reset password ----------

        public void ForgotPassword(string email)
        {
            var user = _repo.GetByEmail(email);

            // Do not reveal whether an email is registered.
            if (user == null)
                return;

            var resetToken = new JwtSecurityToken(
                issuer: Issuer,
                audience: ResetAudience,
                claims: new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("purpose", "reset"),
                    new Claim("pwd", PasswordStamp(user.PasswordHash))
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(15),
                signingCredentials: new SigningCredentials(GetKey(), SecurityAlgorithms.HmacSha256));

            var token = new JwtSecurityTokenHandler().WriteToken(resetToken);

            var subject = "Fundoo - Password Reset Token";
            var body = $"""
        Hello,

        Use the token below to reset your Fundoo password.
        This token expires in 15 minutes and can be used only once.

        Reset token:
        {token}

        If you did not request this, ignore this email.
        """;

            try
            {
                _email.Send(user.Email, subject, body);
                _logger.LogInformation("AUDIT: password reset requested for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                // Mail fail ho to bhi response same rakho, warna pata chal jayega ki email registered hai
                _logger.LogError(ex, "Password reset email send failed for user {UserId}", user.Id);
            }
        }

        public bool ResetPassword(ResetPasswordDto dto)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();

                var principal = handler.ValidateToken(dto.Token, new TokenValidationParameters
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

                if (principal.FindFirst("purpose")?.Value != "reset")
                    return false;

                var email = principal.FindFirst(ClaimTypes.Email)?.Value;
                if (email == null) return false;

                var user = _repo.GetByEmail(email);
                if (user == null) return false;

                // Token ek hi baar chalega: password badalne ke baad stamp match nahi karega
                if (principal.FindFirst("pwd")?.Value != PasswordStamp(user.PasswordHash))
                    return false;

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                user.FailedLoginCount = 0;
                user.LockoutEnd = null;
                _repo.Update(user);

                _logger.LogInformation("AUDIT: password reset completed for user {UserId}", user.Id);
                return true;
            }
            catch
            {
                _logger.LogWarning("AUDIT: password reset failed - invalid or expired token");
                return false;
            }
        }
    }
}