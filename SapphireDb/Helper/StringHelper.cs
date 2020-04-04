using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.FileSystemGlobbing;

namespace SapphireDb.Helper
{
    static class StringHelper
    {
        public static string ToCamelCase(this string input)
        {
            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        public static string ComputeHash(this string input)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(input);
            SHA512 sha512 = SHA512.Create();
            byte[] hash = sha512.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "");
        }
        
        public static bool MatchesGlobPattern(this string input, string globPattern)
        {
            Matcher m = new Matcher();
            m.AddInclude(globPattern);
            return m.Match(input).HasMatches;
        }
    }
}
