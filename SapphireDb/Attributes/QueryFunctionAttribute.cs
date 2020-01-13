using System;
using System.Linq.Expressions;
using System.Reflection;
using SapphireDb.Models;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryFunctionAttribute : Attribute
    {
        public string Function { get; set; }

        public MethodInfo FunctionInfo { get; set; }

        public Func<HttpInformation, dynamic> FunctionBuilder { get; set; }

        public QueryFunctionAttribute(string function)
        {
            Function = function;
        }

        public void Compile(Type modelType)
        {
            if (!string.IsNullOrEmpty(Function))
            {
                FunctionInfo = modelType.GetMethod(Function, BindingFlags.Default|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            }
        }

        public Expression<Func<object, bool>> GetLambda(HttpInformation httpInformation, Type modelType)
        {
            Expression queryFunctionExpression = FunctionBuilder(httpInformation);
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            Expression converted = Expression.Invoke(queryFunctionExpression, Expression.Convert(parameter, modelType));
            return Expression.Lambda<Func<object, bool>>(converted, parameter);
        }
    }
}
