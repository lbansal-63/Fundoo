using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Middleware.Services.Google
{
    public class GoogleTokenValidator : IGoogleTokenValidator
    {
        private readonly IConfiguration _config;
        private readonly ILogger<GoogleTokenValidator> _logger;

        public GoogleTokenValidator(IConfiguration config, ILogger<GoogleTokenValidator> logger)
        {
            _config = config;
            _logger = logger;
        }

        private string ClientId =>
            _config["Google:ClientId"] is { Length: > 0 } id
                ? id
                : throw new InvalidOperationException("Google:ClientId is missing.");

        public async Task<GoogleUserInfo?> ValidateAsync(string idToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(
                    idToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { ClientId }
                    });

                if (string.IsNullOrWhiteSpace(payload.Email))
                    return null;

                var first = payload.GivenName ?? payload.Name ?? "";
                var last = payload.FamilyName ?? "";
                return new GoogleUserInfo(payload.Subject, payload.Email, payload.EmailVerified, first, last);
            }
            catch (InvalidJwtException ex)
            {
                _logger.LogWarning("AUDIT: google login failed - invalid id_token ({Reason})", ex.Message);
                return null;
            }
        }
    }
}