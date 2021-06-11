using System;
using System.Reflection;
using SapphireDb.Helper;
using SapphireDb.Models.SapphireApiBuilder;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class QueryAttribute : QueryAttributeBase, ICompilableAttribute
    {
        public string QueryName { get; }
        
        public QueryAttribute(string queryName, string functionName)
        {
            QueryName = queryName;
            FunctionName = functionName;
        }
        
        public void Compile(Type declaredType, Type modelType = null)
        {
            FunctionInfo = ReflectionMethodHelper.GetMethodInfo(declaredType, FunctionName, typeof(SapphireQueryBuilderBase<>).MakeGenericType(declaredType),
                BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}