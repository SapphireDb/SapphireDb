using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace SapphireDb.Internal.Prefilter
{
    public class IncludePrefilter : IPrefilter
    {
        public string Include { get; set; }

        public List<Type> AffectedModelTypes { get; set; } = new List<Type>();

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

            Type propertyType = modelType;

            foreach (string includePart in Include.Split('.'))
            {
                PropertyInfo propertyInfo = propertyType.GetProperty(includePart);

                if (propertyInfo == null)
                {
                    break;
                }

                propertyType = propertyInfo.PropertyType;

                if (!AffectedModelTypes.Contains(propertyType) && propertyType != modelType)
                {
                    AffectedModelTypes.Add(propertyType);
                }
            }
        }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Include(Include);
        }
    }
}
