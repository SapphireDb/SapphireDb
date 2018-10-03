using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    class WherePrefilter : IPrefilter
    {
        public string PropertyName { get; set; }

        public string Comparision { get; set; }

        public string CompareValue { get; set; }

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
                        if (Comparision == "==")
                        {
                            return array.Where(v => prop.GetValue(v).ToString() == CompareValue);
                        }
                        else if (Comparision == "!=")
                        {
                            return array.Where(v => prop.GetValue(v).ToString() != CompareValue);
                        }
                        else if (Comparision == "<")
                        {
                            return array.Where(v => ((IComparable)CompareValue).CompareTo(prop.GetValue(v).ToString()) > 0);
                        }
                        else if (Comparision == "<=")
                        {
                            return array.Where(v => ((IComparable)CompareValue).CompareTo(prop.GetValue(v).ToString()) >= 0);
                        }
                        else if (Comparision == ">")
                        {
                            return array.Where(v => ((IComparable)CompareValue).CompareTo(prop.GetValue(v).ToString()) < 0);
                        }
                        else if (Comparision == ">=")
                        {
                            return array.Where(v => ((IComparable)CompareValue).CompareTo(prop.GetValue(v).ToString()) <= 0);
                        }
                    }
                }
            }

            return array;
        }
    }
}
