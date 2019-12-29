using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAuthAttribute : AuthAttributeBase
    {
        public CreateAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
