using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace TIPServer
{
    class AuthenticationMethods
    {

        // 24 = 192 bits
        private const int SaltByteSize = 24;
        private const int HashByteSize = 24;
        private const int HasingIterationsCount = 10101;

        internal static byte[] GenerateSalt(int saltByteSize = SaltByteSize)
        {
            using (RNGCryptoServiceProvider saltGenerator = new RNGCryptoServiceProvider())
            {
                byte[] salt = new byte[saltByteSize];
                saltGenerator.GetBytes(salt);
                return salt;
            }
        }

        internal static byte[] ComputeHash(string password, byte[] salt, int iterations = HasingIterationsCount, int hashByteSize = HashByteSize)
        {
            using (Rfc2898DeriveBytes hashGenerator = new Rfc2898DeriveBytes(password, salt))
            {
                hashGenerator.IterationCount = iterations;
                return hashGenerator.GetBytes(hashByteSize);
            }
        }

        internal static string HashPassword(string password)
        {
            byte[] salt = { 83, 248, 250, 68, 180, 164, 188, 217, 106, 213, 245, 250, 198, 75, 208, 102, 0, 45, 83, 253, 109, 149, 225, 139 };

            byte[] hashedPassword = AuthenticationMethods.ComputeHash(password, salt);
            return Convert.ToBase64String(hashedPassword);
        }

    }
}
