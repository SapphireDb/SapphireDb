using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WebUI.Helper
{
    public static class Security
    {
        public static string ComputeHash(this string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            SHA512 sha512 = SHA512.Create();
            byte[] hash = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
