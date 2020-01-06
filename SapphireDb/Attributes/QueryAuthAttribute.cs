using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property, AllowMultiple = true)]
    public class QueryAuthAttribute : AuthAttributeBase
    {
        public QueryAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
