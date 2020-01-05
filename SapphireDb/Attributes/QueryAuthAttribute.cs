using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Property, AllowMultiple = true)]
    public class QueryAuthAttribute : AuthAttributeBase
    {
        public bool PerEntry { get; }
        
        public QueryAuthAttribute(string policies = null, string functionName = null, bool perEntry = false) : base(policies, functionName)
        {
            PerEntry = perEntry;
        }
    }
}
