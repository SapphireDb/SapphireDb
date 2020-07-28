using System;
using System.Reflection;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryAttribute : Attribute
    {
        public string QueryName { get; set; }
        
        public string FunctionName { get; set; }

        public MethodInfo FunctionInfo { get; set; }
        
        public Func<HttpInformation, object[], object> FunctionLambda { get; set; }
        
        public QueryAttribute(string queryName, string functionName)
        {
            QueryName = queryName;
            FunctionName = functionName;
        }
        
        public void Compile(Type modelType)
        {
            FunctionInfo = ReflectionMethodHelper.GetMethodInfo(modelType, FunctionName, typeof(SapphireQueryBuilder<>).MakeGenericType(modelType));
        }
    }
}