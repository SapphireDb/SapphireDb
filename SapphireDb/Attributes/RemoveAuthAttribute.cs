using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RemoveAuthAttribute : AuthAttributeBase
    {
        public RemoveAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
