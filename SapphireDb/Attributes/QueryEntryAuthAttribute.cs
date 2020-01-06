using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class QueryEntryAuthAttribute : AuthAttributeBase
    {
        public QueryEntryAuthAttribute(string policies = null, string functionName = null) : base(policies, functionName)
        {
        }
    }
}
