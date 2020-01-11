using System;
using System.Reflection;

namespace SapphireDb.Attributes
{
    public class AuthAttributeBase : Attribute
    {
        public string[] Policies { get; } = new string[0];

        private string FunctionName { get; }

        public MethodInfo FunctionInfo { get; set; }

        public AuthAttributeBase(string policies = null, string functionName = null)
        {
            if (!string.IsNullOrEmpty(policies))
            {
                Policies = policies.Split(',');
            }

            FunctionName = functionName;
        }

        public void Compile(Type targetType)
        {
            if (string.IsNullOrEmpty(FunctionName))
            {
                return;
            }
            
            FunctionInfo = targetType.GetMethod(FunctionName,
                BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            if (FunctionInfo == null || FunctionInfo.ReturnType != typeof(bool))
            {
                throw new Exception("No suiting method was found");
            }
        }
    }
}
