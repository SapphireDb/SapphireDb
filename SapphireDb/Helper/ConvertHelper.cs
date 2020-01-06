using System;
using System.Globalization;

namespace SapphireDb.Helper
{
    public static class ConvertHelper
    {
        public static object ConvertToTargetType(this string input, Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (string.IsNullOrEmpty(input))
            {
                return Activator.CreateInstance(type);
            }

            if (type == typeof(DateTimeOffset))
            {
                return DateTimeOffset.Parse(input, CultureInfo.InvariantCulture);
            }

            if (type == typeof(TimeSpan))
            {
                return TimeSpan.Parse(input, CultureInfo.InvariantCulture);
            }
            
            if (type == typeof(Guid))
            {
                return Guid.Parse(input);
            }

            return Convert.ChangeType(input, type, CultureInfo.InvariantCulture);
        }
    }
}