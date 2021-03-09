using System;
using System.Reflection;
using SapphireDb.Helper;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class QueryAuthAttribute : AuthAttributeBase
    {
        public QueryAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }

        public override void Compile(Type targetType, CompileContext compileContext)
        {
            if (compileContext == CompileContext.Class)
            {
                FunctionInfo = ReflectionMethodHelper.GetMethodInfo(targetType, FunctionName, typeof(bool),
                    BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            }
            else if (compileContext == CompileContext.Property)
            {
                FunctionInfo = ReflectionMethodHelper.GetMethodInfo(targetType, FunctionName, typeof(bool));
            }
        }
    }
}