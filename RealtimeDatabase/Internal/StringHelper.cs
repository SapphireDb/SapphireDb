using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Internal
{
    static class StringHelper
    {
        public static string ToCamelCase(this string input)
        {
            return Char.ToLowerInvariant(input[0]) + input.Substring(1);
        }
    }
}
