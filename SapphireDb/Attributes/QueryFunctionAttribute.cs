using System;
using System.Reflection;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class QueryFunctionAttribute : Attribute
    {
        public string Function { get; set; }

        public MethodInfo FunctionInfo { get; set; }
        
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
