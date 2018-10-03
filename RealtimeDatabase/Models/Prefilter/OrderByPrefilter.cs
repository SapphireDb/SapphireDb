using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    class OrderByPrefilter : IPrefilter
    {
        public string PropertyName { get; set; }

        public bool Descending { get; set; }

        public IEnumerable<object> Execute(IEnumerable<object> array)
        {
            object firstValue = array.FirstOrDefault();

            if (firstValue != null)
            {
                Type arrayObjectType = firstValue.GetType();

                if (arrayObjectType != null)
                {
                    PropertyInfo prop =
                        arrayObjectType.GetProperties().FirstOrDefault(p => p.Name.ToLowerInvariant() == PropertyName.ToLowerInvariant());

                    if (prop != null)
                    {
                        if (Descending)
                        {
                            return array.OrderByDescending(o => prop.GetValue(o));
                        }
                        else
                        {
                            return array.OrderBy(o => prop.GetValue(o));
                        }
                    }
                }
            }

            return array;
        }
    }
}
