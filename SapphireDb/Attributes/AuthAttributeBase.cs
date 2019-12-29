using System;

namespace SapphireDb.Attributes
{
    public class AuthAttributeBase : Attribute
    {
        public string[] Policies { get; } = new string[0];

        public string FunctionName { get; }

        public AuthAttributeBase(string policies = null, string functionName = null)
        {
            if (!string.IsNullOrEmpty(policies))
            {
                Policies = policies.Split(',');
            }

            FunctionName = functionName;
        }
    }
}
