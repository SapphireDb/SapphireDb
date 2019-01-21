using System;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

        public static string FixCamelCaseAttributeNaming(this string functionString, Type objectType)
        {
            int firstOpenBracketIndex = functionString.IndexOf('(');
            int firstClosingBracketIndex = functionString.IndexOf(')');

            string variableName = functionString.Substring(firstOpenBracketIndex + 1, firstClosingBracketIndex - firstOpenBracketIndex - 1);

            MatchCollection matches = new Regex(variableName + "\\.\\w*").Matches(functionString);

            foreach (Match match in matches)
            {
                string propertyName = match.Value.Substring(match.Value.IndexOf('.') + 1);
                PropertyInfo propertyInfo = objectType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                if (propertyInfo != null)
                {
                    functionString = functionString.Remove(match.Index, match.Length).Insert(match.Index, variableName + "." + propertyInfo.Name);
                }
            }

            return functionString;
        }
    }
}
