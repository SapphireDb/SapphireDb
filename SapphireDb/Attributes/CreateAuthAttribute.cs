using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CreateAuthAttribute : AuthAttributeBase
    {
        public CreateAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
