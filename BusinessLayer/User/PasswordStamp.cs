using System.Security.Cryptography;
using System.Text;

namespace BusinessLayer.User
{
    internal static class PasswordStamp
    {
        // Password badalte hi ye badal jata hai, isliye reset token sirf ek baar chalega
        public static string Create(string passwordHash) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(passwordHash)))[..16];
    }
}