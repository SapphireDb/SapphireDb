using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    class ThenOrderByPrefilter : IPrefilter
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
                        try
                        {
                            IOrderedEnumerable<object> orderedArray = (IOrderedEnumerable<object>)array;

                            if (Descending)
                            {
                                return orderedArray.ThenByDescending(o => prop.GetValue(o));
                            }
                            else
                            {
                                return orderedArray.ThenBy(o => prop.GetValue(o));
                            }
                        }
                        catch
                        {
                            
                        }
                        
                    }
                }
            }

            return array;
        }
    }
}
