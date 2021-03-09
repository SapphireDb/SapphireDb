using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DeleteAuthAttribute : AuthAttributeBase
    {
        public DeleteAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
