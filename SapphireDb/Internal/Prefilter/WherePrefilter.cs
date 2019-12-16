using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JavaScriptEngineSwitcher.Core;
using Newtonsoft.Json.Linq;
using SapphireDb.Helper;

namespace SapphireDb.Internal.Prefilter
{
    public class WherePrefilter : IPrefilter
    {
        public List<JToken> Conditions { get; set; }

        public Expression<Func<object, bool>> WhereExpression { get; set; }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Where(WhereExpression);
        }

        private Expression CreateCompareExpression(Type modelType, JArray compareParts, Expression modelExpression)
        {
            PropertyInfo compareProperty = modelType.GetProperty(compareParts.First().Value<string>(),
                BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                BindingFlags.Instance);

            if (compareProperty == null)
            {
                return Expression.Constant(true);
            }

            MemberExpression propertyExpression = Expression.PropertyOrField(modelExpression, compareProperty.Name);

            

            object compareValue = typeof(Newtonsoft.Json.Linq.Extensions).GetMethods().FirstOrDefault(m => m.Name == "Value")?.MakeGenericMethod(compareProperty.PropertyType)
                .Invoke(null, new object[] { compareParts.Last() });

            Expression compareValueExpression = Expression.Constant(compareValue);

            
            switch (compareParts.Skip(1).First().Value<string>())
            {
                case "Contains":
                    return ExpressionHelper.Contains(propertyExpression, compareValueExpression);
                case "StartsWith":
                    return ExpressionHelper.StartsWith(propertyExpression, compareValueExpression);
                case "EndsWith":
                    return ExpressionHelper.EndsWith(propertyExpression, compareValueExpression);
                case "==":
                default:
                    return Expression.Equal(propertyExpression, compareValueExpression);
            }
        }

        private bool initialized = false;

        public void Initialize(Type modelType)
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            ParameterExpression parameter = Expression.Parameter(typeof(object));
            UnaryExpression modelExpression = Expression.Convert(parameter, modelType);

            Expression completeExpression = Expression.Empty();

            foreach (JToken conditionPart in Conditions)
            {
                if (conditionPart.Type == JTokenType.Array)
                {

                }
                else if (conditionPart.Type == JTokenType.String)
                {
                    
                }
            }

            Expression t = CreateCompareExpression(modelType, Conditions.First().Value<JArray>(), modelExpression);

            WhereExpression = Expression.Lambda<Func<object, bool>>(t, parameter);
        }

        public void Dispose()
        {
            
        }
    }
}
