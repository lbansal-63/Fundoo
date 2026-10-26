using Microsoft.Extensions.Logging;
using Middleware.Services.Hashing;
using Middleware.Services.Jwt;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.User.Queries
{
    public class LoginUserHandler
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

        // "Unknown email" aur "wrong password" ka time lagbhag same rakhne ke liye
        // (ye ek valid hash hai, kisi real user ka nahi)
        private const string DummyHash =
            "600000.8IIL89tZAKgP2nK6Pr+Slw==.OTGeAkhbOui5kpZ8TSnnEwT6e15Lr2SWGYBjZoQ5s7A=";

        private readonly IUserRepository _repo;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenService _jwt;
        private readonly ILogger<LoginUserHandler> _logger;

        public LoginUserHandler(
            IUserRepository repo,
            IPasswordHasher hasher,
            IJwtTokenService jwt,
            ILogger<LoginUserHandler> logger)
        {
            _repo = repo;
            _hasher = hasher;
            _jwt = jwt;
            _logger = logger;
        }

        public string? Handle(LoginUserQuery query)
        {
            var email = query.Email.Trim().ToLowerInvariant();
            var user = _repo.GetByEmail(email);

            if (user == null)
            {
                _hasher.Verify(query.Password, DummyHash); // timing similar rakhne ke liye
                _logger.LogWarning("AUDIT: login failed - unknown email");
                return null;
            }

            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
            {
                _logger.LogWarning("AUDIT: login blocked - user {UserId} is locked out", user.Id);
                return null; // wrong password jaisa hi generic failure
            }

            if (!_hasher.Verify(query.Password, user.PasswordHash))
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
    }
}