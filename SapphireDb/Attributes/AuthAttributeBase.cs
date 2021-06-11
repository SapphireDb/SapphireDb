using System;
using System.Reflection;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Attributes
{
    public abstract class AuthAttributeBase : Attribute
    {
        public string[] Policies { get; } = new string[0];

        protected string FunctionName { get; }

        public MethodInfo FunctionInfo { get; set; }
        
        public Func<HttpInformation, dynamic, bool> FunctionLambda { get; set; }

        public AuthAttributeBase(string policies = null, string functionName = null)
        {
            if (!string.IsNullOrEmpty(policies))
            {
                Policies = policies.Split(',');
            }

            FunctionName = functionName;
        }

        public virtual void Compile(Type targetType, CompileContext compileContext)
        {
            FunctionInfo = ReflectionMethodHelper.GetMethodInfo(targetType, FunctionName, typeof(bool));
        }

        public enum CompileContext
        {
            Class, Method, Property
        }
    }
}
