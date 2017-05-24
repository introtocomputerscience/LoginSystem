using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace CustomLoginSystem.Helpers
{
    public class HashHelper
    {
        public static byte[] ComputeHash(byte[] input, byte[] salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();

            byte[] inputWithSaltBytes = new byte[input.Length + salt.Length];

            for(int i = 0; i < input.Length; i++)
            {
                inputWithSaltBytes[i] = input[i];
            }
            for(int i = 0; i < salt.Length; i++)
            {
                inputWithSaltBytes[input.Length + 1] = salt[i];
            }

            return algorithm.ComputeHash(inputWithSaltBytes);
        }

        public static (Guid salt, byte[] encryptedPassword) GetEncryptedPassword(string password, string secretKey)
        {
            var salt = Guid.NewGuid();
            var encryptedPassword = GetEncryptedPassword(salt, password, secretKey);
            return (salt, encryptedPassword);
        }

        public static byte[] GetEncryptedPassword(Guid salt, string password, string secretKey)
        {
            var saltedPassword = HashHelper.ComputeHash(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt.ToString()));
            var encryptedPassword = HashHelper.ComputeHash(saltedPassword, Encoding.UTF8.GetBytes(secretKey));
            return encryptedPassword;
        }
    }
}