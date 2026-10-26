using Microsoft.Extensions.Logging;
using Middleware.Services.Hashing;
using RepositoryLayer.Entities;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.User.Commands
{
    public class RegisterUserHandler
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasher _hasher;
        private readonly ILogger<RegisterUserHandler> _logger;

        public RegisterUserHandler(
            IUserRepository repo,
            IPasswordHasher hasher,
            ILogger<RegisterUserHandler> logger)
        {
            _repo = repo;
            _hasher = hasher;
            _logger = logger;
        }

        // true = register ho gaya, false = email pehle se hai
        public bool Handle(RegisterUserCommand command)
        {
            var email = command.Email.Trim().ToLowerInvariant();

            if (_repo.GetByEmail(email) != null)
                return false;

            _repo.Add(new UserEntity
            {
                FirstName = command.FirstName,
                LastName = command.LastName,
                Email = email,
                PhoneNumber = command.PhoneNumber.Trim(),
                PasswordHash = _hasher.Hash(command.Password)
            });

            _logger.LogInformation("AUDIT: new user registered");
            return true;
        }
    }
}