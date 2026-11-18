using System.Security.Cryptography;

namespace Middleware.Services.Hashing
{
    // Format jo DB mein store hota hai:  iterations.salt.hash  (salt aur hash Base64 mein)
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;      // 128-bit random salt
        private const int HashSize = 32;      // 256-bit hash
        private const int Iterations = 600_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        public string Hash(string password)
        {
            // Har password ke liye naya random salt
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool Verify(string password, string storedHash)
        {
            var parts = storedHash.Split('.');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations) || iterations <= 0)
                return false;

            try
            {
                var salt = Convert.FromBase64String(parts[1]);
                var expected = Convert.FromBase64String(parts[2]);

                // Wahi salt aur iterations se dobara hash, phir compare
                var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, expected.Length);

                // Constant-time compare, taaki timing se hash ka andaza na lage
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}