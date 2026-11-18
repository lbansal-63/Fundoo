namespace Middleware.Services.Hashing
{
    public interface IPasswordHasher
    {
        // Password ko hash karta hai (DB mein ye hash store hoga)
        string Hash(string password);

        // Diya gaya password us hash se match karta hai ya nahi
        bool Verify(string password, string hash);
    }
}