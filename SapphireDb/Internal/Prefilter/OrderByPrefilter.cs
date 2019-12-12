using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JavaScriptEngineSwitcher.Core;
using SapphireDb.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace SapphireDb.Internal.Prefilter
{
    public class OrderByPrefilter : IPrefilter
    {
        public string Property { get; set; }

        protected Expression<Func<object, object>> PropertySelectExpression { get; set; }

        public bool Descending { get; set; }


        public bool Initialized { get; set; }

        public void Initialize(Type modelType)
        {
            Initialized = true;

            string propertyName = modelType.GetProperty(Property, BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase | BindingFlags.Instance)?.Name;

            if (!string.IsNullOrEmpty(propertyName))
            {
                ParameterExpression parameter = Expression.Parameter(typeof(object), "x");
                UnaryExpression convertExpression = Expression.Convert(parameter, modelType);
                
                MemberExpression body = Expression.PropertyOrField(convertExpression, propertyName);
                PropertySelectExpression = Expression.Lambda<Func<object, object>>(body, parameter);
            }
        }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return Descending
                ? array.OrderByDescending(PropertySelectExpression)
                : array.OrderBy(PropertySelectExpression);
        }

        public void Dispose()
        {
            
        }
    }
}
