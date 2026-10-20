namespace Middleware.Services.Jwt
{
    public record ResetTokenInfo(string Email, string PasswordStamp);

    public interface IJwtTokenService
    {
        string CreateAccessToken(int userId, string email, int tokenVersion);
        string CreateResetToken(string email, string passwordStamp);

        ResetTokenInfo? ValidateResetToken(string token);
    }
}
