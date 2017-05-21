using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    }
}