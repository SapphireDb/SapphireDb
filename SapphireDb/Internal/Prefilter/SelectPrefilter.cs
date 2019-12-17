using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Helper;

namespace SapphireDb.Internal.Prefilter
{
    public class SelectPrefilter : IAfterQueryPrefilter
    {
        public List<string> Properties { get; set; }

        private Expression<Func<object, object>> selectExpression;

        public object Execute(IQueryable<object> array)
        {
            return array.Select(selectExpression);
        }

        private bool initialized;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            ParameterExpression parameter = Expression.Parameter(typeof(object));
            UnaryExpression modelExpression = Expression.Convert(parameter, modelType);

            Expression body;

            if (Properties.Count == 1)
            {
                string propertyName = modelType.GetProperty(Properties[0],
                    BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                    BindingFlags.Instance)?.Name;

                if (string.IsNullOrEmpty(propertyName))
                {
                    body = Expression.Constant(null);
                }
                else
                {
                    body = Expression.PropertyOrField(modelExpression, propertyName);
                }
            }
            else
            {
                IEnumerable<UnaryExpression> propertyExpressions = Properties
                    .Select(propertyName => modelType.GetProperty(propertyName,
                        BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                        BindingFlags.Instance))
                    .Where(property => property != null)
                    .Select((property, index) =>
                    {
                        MemberExpression propertyExpression = Expression.PropertyOrField(modelExpression, property.Name);
                        return Expression.Convert(propertyExpression, typeof(object));
                    });

                body = Expression.NewArrayInit(typeof(object), propertyExpressions);
            }

            selectExpression = Expression.Lambda<Func<object, object>>(body, parameter);
        }

        public void Dispose()
        {
            
        }
    }
}
