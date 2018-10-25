using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class QueryAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public QueryAuthAttribute()
        {

        }

        public QueryAuthAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
