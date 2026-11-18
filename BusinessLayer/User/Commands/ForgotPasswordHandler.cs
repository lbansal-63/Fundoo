using Microsoft.Extensions.Logging;
using Middleware.Services.Email;
using Middleware.Services.Jwt;
using RepositoryLayer.Interfaces;

namespace BusinessLayer.User.Commands
{
    public class ForgotPasswordHandler
    {
        private readonly IUserRepository _repo;
        private readonly IJwtTokenService _jwt;
        private readonly IEmailSender _email;
        private readonly ILogger<ForgotPasswordHandler> _logger;

        public ForgotPasswordHandler(
            IUserRepository repo,
            IJwtTokenService jwt,
            IEmailSender email,
            ILogger<ForgotPasswordHandler> logger)
        {
            _repo = repo;
            _jwt = jwt;
            _email = email;
            _logger = logger;
        }

        public void Handle(ForgotPasswordCommand command)
        {
            var user = _repo.GetByEmail(command.Email.Trim().ToLowerInvariant());

            // Do not reveal whether an email is registered.
            if (user == null)
                return;

            var token = _jwt.CreateResetToken(user.Email, PasswordStamp.Create(user.PasswordHash));

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
    }
}