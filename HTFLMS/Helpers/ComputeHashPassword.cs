using System;
using System.Security.Cryptography;

namespace HTFLMS.Helper
{
    public class ComputeHashPassword
    {
        public static Byte[] PasswordHashKey(string password)
        {
            byte[] passwordKey;

            using (var hmac = new HMACSHA512())
            {
                passwordKey = hmac.Key;
            }

            return passwordKey;
        }

        public static Byte[] PasswordHash(string password)
        {
            byte[] passwordHash;

            using (var hmac = new HMACSHA512())
            {
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }

            return passwordHash;
        }
    }
}
