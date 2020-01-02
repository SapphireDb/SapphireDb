using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SapphireDb.Helper
{
    public static class ExpressionHelper
    {
        public static MethodCallExpression ToLower(Expression input)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.ToLower), new Type[] { });
            return Expression.Call(input, method);
        }
        
        public static MethodCallExpression Contains(Expression input, Expression checkString)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            return Expression.Call(input, method, checkString);
        }
        
        public static MethodCallExpression ContainsCaseInsensitive(Expression input, Expression checkString)
        {
            return Contains(ToLower(input), ToLower(checkString));
        }

        public static MethodCallExpression StartsWith(Expression input, Expression checkString)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
            return Expression.Call(input, method, checkString);
        }

        public static MethodCallExpression StartsWithCaseInsensitive(Expression input, Expression checkString)
        {
            return StartsWith(ToLower(input), ToLower(checkString));
        }
        
        public static MethodCallExpression EndsWith(Expression input, Expression checkString)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
            return Expression.Call(input, method, checkString);
        }
        
        public static MethodCallExpression EndsWithCaseInsensitive(Expression input, Expression checkString)
        {
            return EndsWith(ToLower(input), ToLower(checkString));
        }

        public static MethodCallExpression ArrayContains(Expression input, Expression checkValue)
        {
            MethodInfo method = typeof(IList).GetMethod(nameof(IList.Contains));
            return Expression.Call(input, method, checkValue);
        }
        
        public static MethodCallExpression InArray(Expression input, Expression checkArray)
        {
            return ArrayContains(checkArray, input);
        }
        
        public static Expression CreateCompareExpression(Type modelType, JArray compareParts, Expression modelExpression)
        {
            PropertyInfo compareProperty = modelType.GetProperty(compareParts.First().Value<string>(),
                BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase |
                BindingFlags.Instance);

            if (compareProperty == null)
            {
                return Expression.Constant(true);
            }

            MemberExpression propertyExpression = Expression.PropertyOrField(modelExpression, compareProperty.Name);

            string compareOperation = compareParts.Skip(1).First().Value<string>();
            object compareValue;
            
            if (compareOperation == "InArray")
            {
                object compareValues = (IEnumerable)typeof(JContainer).GetMethods()
                    .FirstOrDefault(m => m.Name == nameof(JContainer.Values))?
                    .MakeGenericMethod(compareProperty.PropertyType)
                    .Invoke(compareParts.Last(), new object[] { });

                compareValue = typeof(Enumerable)
                    .GetMethod(nameof(Enumerable.ToList))?
                    .MakeGenericMethod(compareProperty.PropertyType)
                    .Invoke(null, new object[] { compareValues });
            }
            else
            {
                compareValue = typeof(Newtonsoft.Json.Linq.Extensions).GetMethods()
                    .FirstOrDefault(m => m.Name == "Value")?
                    .MakeGenericMethod(compareProperty.PropertyType)
                    .Invoke(null, new object[] { compareParts.Last() });   
            }

            Expression compareValueExpression = Expression.Constant(compareValue);

            switch (compareOperation)
            {
                case "Contains":
                    return Contains(propertyExpression, compareValueExpression);
                case "ContainsCaseInsensitive":
                    return ContainsCaseInsensitive(propertyExpression, compareValueExpression);
                case "StartsWith":
                    return StartsWith(propertyExpression, compareValueExpression);
                case "StartsWithCaseInsensitive":
                    return StartsWithCaseInsensitive(propertyExpression, compareValueExpression);
                case "EndsWith":
                    return EndsWith(propertyExpression, compareValueExpression);
                case "EndsWithCaseInsensitive":
                    return EndsWithCaseInsensitive(propertyExpression, compareValueExpression);
                case "ArrayContains":
                    return ArrayContains(propertyExpression, compareValueExpression);
                case "InArray":
                    return InArray(propertyExpression, compareValueExpression);
                case "NotEqualIgnoreCase":
                    return Expression.NotEqual(ToLower(propertyExpression), ToLower(compareValueExpression));
                case "EqualIgnoreCase":
                    return Expression.Equal(ToLower(propertyExpression), ToLower(compareValueExpression));
                case "!=":
                    return Expression.NotEqual(propertyExpression, compareValueExpression);
                case "<":
                    return Expression.LessThan(propertyExpression, compareValueExpression);
                case "<=":
                    return Expression.LessThanOrEqual(propertyExpression, compareValueExpression);
                case ">":
                    return Expression.GreaterThan(propertyExpression, compareValueExpression);
                case ">=":
                    return Expression.GreaterThanOrEqual(propertyExpression, compareValueExpression);
                case "==":
                default:
                    return Expression.Equal(propertyExpression, compareValueExpression);
            }
        }

        public static  Expression ConvertConditionParts(Type modelType, JToken conditionParts, Expression modelExpression)
        {
            if (conditionParts.Type == JTokenType.Array)
            {
                if (conditionParts.First().Type == JTokenType.Array)
                {
                    Expression completeExpression = null;
                    Expression prevExpression = null;

                    foreach (JToken combineOperator in conditionParts.Where(t => t.Type == JTokenType.String))
                    {
                        if (prevExpression == null)
                        {
                            prevExpression = ConvertConditionParts(modelType, combineOperator.Previous, modelExpression);
                        }
                        else
                        {
                            prevExpression = completeExpression;
                        }

                        Expression nextExpression = ConvertConditionParts(modelType, combineOperator.Next, modelExpression);

                        string operatorValue = combineOperator.Value<string>();

                        if (operatorValue == "and")
                        {
                            completeExpression = Expression.AndAlso(prevExpression, nextExpression);
                        }
                        else
                        {
                            completeExpression = Expression.OrElse(prevExpression, nextExpression);
                        }
                    }

                    if (completeExpression == null)
                    {
                        completeExpression = ConvertConditionParts(modelType, conditionParts.First(), modelExpression);
                    }

                    return completeExpression;
                }
                else
                {
                    return ExpressionHelper.CreateCompareExpression(modelType, conditionParts.Value<JArray>(), modelExpression);
                }
            }

            throw new Exception("Wrong order of conditions");
        }
    }
}
