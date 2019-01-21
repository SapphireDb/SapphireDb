using System;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class QueryAuthAttribute : AuthAttributeBase
    {
        public QueryAuthAttribute()
        {

        }

        public QueryAuthAttribute(string[] roles) : base(roles)
        {
        }

        public QueryAuthAttribute(string function) : base(function)
        {
        }
    }
}
