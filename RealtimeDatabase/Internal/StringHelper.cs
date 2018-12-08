using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace RealtimeDatabase.Internal
{
    static class StringHelper
    {
        public static string ToCamelCase(this string input)
        {
            return Char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        public static string ComputeHash(this string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            SHA512 sha512 = SHA512.Create();
            byte[] hash = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
