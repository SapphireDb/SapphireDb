using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property)]
    public class QueryAuthAttribute : AuthAttributeBase
    {
        public QueryAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
