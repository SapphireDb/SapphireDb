using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class ActionAuthAttribute : AuthAttributeBase
    {
        public ActionAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
