using System;
using System.Reflection;
using Newtonsoft.Json.Linq;
using SapphireDb.Helper;
using SapphireDb.Models;
using SapphireDb.Models.SapphireApiBuilder;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class QueryAttribute : Attribute
    {
        public string QueryName { get; set; }
        
        public string FunctionName { get; set; }

        public MethodInfo FunctionInfo { get; set; }
        
        public Func<dynamic, HttpInformation, JToken[], dynamic> FunctionLambda { get; set; }
        
        public QueryAttribute(string queryName, string functionName)
        {
            QueryName = queryName;
            FunctionName = functionName;
        }
        
        public void Compile(Type modelType)
        {
            FunctionInfo = ReflectionMethodHelper.GetMethodInfo(modelType, FunctionName, typeof(SapphireQueryBuilderBase<>).MakeGenericType(modelType),
                BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}