namespace Middleware.Services.Google
{
    public record GoogleUserInfo(
        string Subject,
        string Email,
        bool EmailVerified,
        string FirstName,
        string LastName);

    public interface IGoogleTokenValidator
    {
        // Google ka id_token verify karta hai; invalid/expired/galat audience ho to null
        Task<GoogleUserInfo?> ValidateAsync(string idToken);
    }
}