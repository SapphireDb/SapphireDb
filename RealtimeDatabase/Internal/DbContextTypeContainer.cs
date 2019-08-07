using System;
using System.Collections.Generic;
using System.Linq;

namespace RealtimeDatabase.Internal
{
    public class DbContextTypeContainer
    {
        private Dictionary<string, Type> DbContextTypes { get; set; } = new Dictionary<string, Type>();

        public void AddContext(string name, Type type)
        {
            name = name.ToLowerInvariant();

            if (!DbContextTypes.TryAdd(name, type))
            {
                throw new Exception("The context name is already used");
            }
        }

        public Type GetContext(string name)
        {
            name = name.ToLowerInvariant();

            if (DbContextTypes.TryGetValue(name, out Type result))
            {
                return result;
            }

            throw new Exception($"No context with the name '{name}' was found");
        }

        public string GetName(Type type)
        {
            KeyValuePair<string, Type> property = DbContextTypes.FirstOrDefault(v => v.Value == type);
            return property.Key;
        }
    }
}
