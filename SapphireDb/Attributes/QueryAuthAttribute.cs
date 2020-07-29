using System;
using System.Reflection;
using SapphireDb.Helper;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property, AllowMultiple = true)]
    public class QueryAuthAttribute : AuthAttributeBase
    {
        public QueryAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
        
        public new void Compile(Type targetType)
        {
            FunctionInfo = ReflectionMethodHelper.GetMethodInfo(targetType, FunctionName, typeof(bool),
                BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
