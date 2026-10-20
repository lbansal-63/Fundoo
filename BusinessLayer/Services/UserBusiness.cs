using System.Security.Cryptography;
using System.Text;
using BusinessLayer.Interfaces;
using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;
using Middleware.Services.Email;
using Middleware.Services.Hashing;
using Middleware.Services.Jwt;
using ModelLayer;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.Services
{
    public class UserBusiness : IUserBusiness
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan TokenVersionTtl = TimeSpan.FromMinutes(5);

        // "Unknown email" aur "wrong password" ka time lagbhag same rakhne ke liye
        // (ye ek valid BCrypt hash hai, kisi real user ka nahi)
        private const string DummyHash =
            "$2a$11$nOHN4gyDSO/gpAIjpuzjXuPaKZNsIjHz3X6iN2aeVpOZUY//tPBT2";

        private readonly IUserRepository _repo;
        private readonly IJwtTokenService _jwt;
        private readonly IEmailSender _email;
        private readonly IPasswordHasher _hasher;
        private readonly ICacheService _cache;
        private readonly ILogger<UserBusiness> _logger;

        public UserBusiness(
            IUserRepository repo,
            IJwtTokenService jwt,
            IEmailSender email,
            IPasswordHasher hasher,
            ICacheService cache,
            ILogger<UserBusiness> logger)
        {
            _repo = repo;
            _jwt = jwt;
            _email = email;
            _hasher = hasher;
            _cache = cache;
            _logger = logger;
        }

        // Password badalte hi ye badal jata hai, isliye reset token sirf ek baar chalega
        private static string PasswordStamp(string passwordHash) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)))[..16];

        private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

        // Token version badalte hi Redis mein naya version likh do, taaki purane tokens turant revoke ho jayein
        private void RefreshTokenVersionCache(UserEntity user) =>
            _cache.Set(CacheKeys.TokenVersion(user.Id), user.TokenVersion, TokenVersionTtl);

        // ---------- register ----------

        public bool Register(RegisterDto dto)
        {
            var email = NormalizeEmail(dto.Email);

            if (_repo.GetByEmail(email) != null)
                return false;

            _repo.Add(new UserEntity
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = email,
                PhoneNumber = dto.PhoneNumber.Trim(),
                PasswordHash = _hasher.Hash(dto.Password)
            });

            _logger.LogInformation("AUDIT: new user registered");
            return true;
        }

        // ---------- login ----------

        public string? Login(LoginDto dto)
        {
            var user = _repo.GetByEmail(NormalizeEmail(dto.Email));

            if (user == null)
            {
                _hasher.Verify(dto.Password, DummyHash); // timing similar rakhne ke liye
                _logger.LogWarning("AUDIT: login failed - unknown email");
                return null;
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("AUDIT: login blocked - user {UserId} is locked out", user.Id);
                return null; // wrong password jaisa hi generic failure
            }

            if (!_hasher.Verify(dto.Password, user.PasswordHash))
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
            return _jwt.CreateAccessToken(user.Id, user.Email, user.TokenVersion);
        }

        // ---------- forgot / reset password ----------

        public void ForgotPassword(string email)
        {
            var user = _repo.GetByEmail(NormalizeEmail(email));

            // Do not reveal whether an email is registered.
            if (user == null)
                return;

            var token = _jwt.CreateResetToken(user.Email, PasswordStamp(user.PasswordHash));

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
            var info = _jwt.ValidateResetToken(dto.Token);
            if (info == null)
            {
                _logger.LogWarning("AUDIT: password reset failed - invalid or expired token");
                return false;
            }

            var user = _repo.GetByEmail(info.Email);
            if (user == null) return false;

            // Token ek hi baar chalega: password badalne ke baad stamp match nahi karega
            if (info.PasswordStamp != PasswordStamp(user.PasswordHash))
            {
                _logger.LogWarning("AUDIT: password reset failed - invalid or expired token");
                return false;
            }

            user.PasswordHash = _hasher.Hash(dto.NewPassword);
            user.TokenVersion++; // reset ke baad saare purane login tokens revoke
            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            _repo.Update(user);
            RefreshTokenVersionCache(user);

            _logger.LogInformation("AUDIT: password reset completed for user {UserId}", user.Id);
            return true;
        }
    }
}