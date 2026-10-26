using Microsoft.Extensions.Logging;
using Middleware.Services.Cache;
using Middleware.Services.Hashing;
using Middleware.Services.Jwt;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.User.Commands
{
    public class ResetPasswordHandler
    {
        private static readonly TimeSpan TokenVersionTtl = TimeSpan.FromMinutes(5);

        private readonly IUserRepository _repo;
        private readonly IJwtTokenService _jwt;
        private readonly IPasswordHasher _hasher;
        private readonly ICacheService _cache;
        private readonly ILogger<ResetPasswordHandler> _logger;

        public ResetPasswordHandler(
            IUserRepository repo,
            IJwtTokenService jwt,
            IPasswordHasher hasher,
            ICacheService cache,
            ILogger<ResetPasswordHandler> logger)
        {
            _repo = repo;
            _jwt = jwt;
            _hasher = hasher;
            _cache = cache;
            _logger = logger;
        }

        // true = password badal gaya, false = token galat ya expired
        public bool Handle(ResetPasswordCommand command)
        {
            var info = _jwt.ValidateResetToken(command.Token);
            if (info == null)
            {
                _logger.LogWarning("AUDIT: password reset failed - invalid or expired token");
                return false;
            }

            var user = _repo.GetByEmail(info.Email);
            if (user == null)
                return false;

            // Token ek hi baar chalega: password badalne ke baad stamp match nahi karega
            if (info.PasswordStamp != PasswordStamp.Create(user.PasswordHash))
            {
                _logger.LogWarning("AUDIT: password reset failed - invalid or expired token");
                return false;
            }

            user.PasswordHash = _hasher.Hash(command.NewPassword);
            user.TokenVersion++; // reset ke baad saare purane login tokens revoke
            user.FailedLoginCount = 0;
            user.LockoutEnd = null;
            _repo.Update(user);

            // Redis mein naya version, taaki purane tokens turant reject hon
            _cache.Set(CacheKeys.TokenVersion(user.Id), user.TokenVersion, TokenVersionTtl);

            _logger.LogInformation("AUDIT: password reset completed for user {UserId}", user.Id);
            return true;
        }
    }
}