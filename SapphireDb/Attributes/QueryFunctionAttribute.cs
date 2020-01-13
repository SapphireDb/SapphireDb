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
            FunctionInfo = modelType.GetMethod(Function, BindingFlags.Default|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
        }
    }
}
