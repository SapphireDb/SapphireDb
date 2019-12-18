using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SapphireDb.Helper
{
    static class DbContextHelper
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<Type, string>> DbContextSets = new ConcurrentDictionary<Type, Dictionary<Type, string>>();

        public static Dictionary<Type, string> GetDbSetTypes(this Type dbContextType)
        {
            if (!DbContextSets.ContainsKey(dbContextType))
            {
                Dictionary<Type, string> sets = dbContextType.GetProperties()
                    .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                    .ToDictionary(p => p.PropertyType.GenericTypeArguments.FirstOrDefault(), p => p.Name);

                DbContextSets.TryAdd(dbContextType, sets);
            }

            return DbContextSets[dbContextType];
        }

        public static KeyValuePair<Type, string> GetDbSetType(this Type dbContextType, string collectionName)
        {
            return dbContextType.GetDbSetTypes().FirstOrDefault(v => string.Equals(v.Value, collectionName, StringComparison.InvariantCultureIgnoreCase));
        }

        public static KeyValuePair<Type, string> GetCollectionName(this Type modelType)
        {
            return DbContextSets.FirstOrDefault(dbSet => dbSet.Value.ContainsKey(modelType)).Value
                .FirstOrDefault(collection => collection.Key == modelType);
        }
    }
}
