using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace SapphireDb.Helper
{
    public static class ExpressionHelper
    {
        public static MethodCallExpression Contains(Expression input, Expression checkString)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            return Expression.Call(input, method, checkString);
        }

        public static MethodCallExpression StartsWith(Expression input, Expression checkString)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.StartsWith), new[] { typeof(string) });
            return Expression.Call(input, method, checkString);
        }

        public static MethodCallExpression EndsWith(Expression input, Expression checkString)
        {
            MethodInfo method = typeof(string).GetMethod(nameof(string.EndsWith), new[] { typeof(string) });
            return Expression.Call(input, method, checkString);
        }
    }
}
