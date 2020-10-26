using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Helper
{
    public static class ExpressionHelper
    {
        public static MethodCallExpression ToLower(Expression input)
        {
            return Expression.Call(input, ReflectionMethodHelper.StringToLower);
        }
        
        public static MethodCallExpression Contains(Expression input, Expression checkString)
        {
            return Expression.Call(input, ReflectionMethodHelper.StringContains, checkString);
        }
        
        public static MethodCallExpression ContainsCaseInsensitive(Expression input, Expression checkString)
        {
            return Contains(ToLower(input), ToLower(checkString));
        }

        public static MethodCallExpression StartsWith(Expression input, Expression checkString)
        {
            return Expression.Call(input, ReflectionMethodHelper.StringStartsWith, checkString);
        }

        public static MethodCallExpression StartsWithCaseInsensitive(Expression input, Expression checkString)
        {
            return StartsWith(ToLower(input), ToLower(checkString));
        }
        
        public static MethodCallExpression EndsWith(Expression input, Expression checkString)
        {
            return Expression.Call(input, ReflectionMethodHelper.StringEndsWith, checkString);
        }
        
        public static MethodCallExpression EndsWithCaseInsensitive(Expression input, Expression checkString)
        {
            return EndsWith(ToLower(input), ToLower(checkString));
        }

        public static MethodCallExpression ArrayContains(Expression input, Expression checkValue, Type propertyType)
        {
            MethodInfo method = typeof(List<>).MakeGenericType(propertyType).GetMethod(nameof(Contains));
            return Expression.Call(input, method, checkValue);
        }
        
        public static MethodCallExpression InArray(Expression input, Expression checkArray, Type propertyType)
        {
            return ArrayContains(checkArray, input, propertyType);
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

            Type targetType = compareProperty.PropertyType;
            
            if (compareOperation == "InArray")
            {
                targetType = typeof(List<>).MakeGenericType(targetType);
            }

            object compareValue = compareParts.Last.ToObject(targetType);
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
                    return ArrayContains(propertyExpression, compareValueExpression, compareProperty.PropertyType);
                case "InArray":
                    return InArray(propertyExpression, compareValueExpression, compareProperty.PropertyType);
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

        public static Expression ConvertConditionParts(Type modelType, JToken conditionParts, Expression modelExpression)
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
                
                return CreateCompareExpression(modelType, conditionParts.Value<JArray>(), modelExpression);
            }

            throw new WrongConditionOrderException(conditionParts);
        }

        public static string ToString(this Expression expression, object expressionCompiled)
        {
            string expressionString = expression.ToString();

            object target = expressionCompiled.GetType().GetProperty("Target")?.GetValue(expressionCompiled);
            
            if (target?.GetType().GetField("Constants")?.GetValue(target) is object[] constants)
            {
                foreach (object constantContainer in constants)
                {
                    Type objectType = constantContainer.GetType();
                    string objectName = objectType.FullName;

                    foreach (FieldInfo field in objectType.GetFields().OrderByDescending(f => f.Name))
                    {
                        object value = field.GetValue(constantContainer);
                        string valueString = Expression.Constant(value).ToString();
                        string placeholder = $"value({objectName}).{field.Name}";
                        expressionString = expressionString.Replace(placeholder, valueString);
                    }
                }
            }

            return expressionString;
        }
    }
}
