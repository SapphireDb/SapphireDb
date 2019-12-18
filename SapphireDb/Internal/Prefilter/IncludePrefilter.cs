using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SapphireDb.Helper;

namespace SapphireDb.Internal.Prefilter
{
    public class IncludePrefilter : IPrefilter
    {
        public string Include { get; set; }

        public List<string> AffectedCollectionNames { get; set; }

        public void Dispose()
        {
            
        }

        private bool initialized;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            List<Type> affectedTypes = new List<Type>();

            Type propertyType = modelType;

            foreach (string includePart in Include.Split('.'))
            {
                PropertyInfo propertyInfo = propertyType.GetProperties().FirstOrDefault(p => p.Name.Equals(includePart, StringComparison.InvariantCultureIgnoreCase));

                if (propertyInfo == null)
                {
                    break;
                }


                propertyType = typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType) ? propertyInfo.PropertyType.GenericTypeArguments[0] : propertyInfo.PropertyType;

                if (!affectedTypes.Contains(propertyType) && propertyType != modelType)
                {
                    affectedTypes.Add(propertyType);
                }
            }

            AffectedCollectionNames = affectedTypes.Select(collectionType => collectionType.GetCollectionName().Value).ToList();
        }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Include(Include);
        }
    }
}
