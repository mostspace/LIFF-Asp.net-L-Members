using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using System.Text;

namespace nxLINEadmin.Utilities
{
    public static class AuthHelper
    {
        public static bool CheckPasswordLogin(string password, string hashPassword)
        {
            var passwordHash = HashPassword(password);
            var result = PasswordVerificationResult.Failed;
            if (passwordHash.Equals(hashPassword))
            {
                result = PasswordVerificationResult.Success;
            }

            return result == PasswordVerificationResult.Success;
        }

        public static string HashPassword(string password)
        {
            using (SHA256 mySHA256 = SHA256.Create())
            {
                return System.Convert.ToBase64String(mySHA256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}
