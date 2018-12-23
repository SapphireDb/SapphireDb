using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class QueryAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public string FunctionName { get; set; }

        public QueryAuthAttribute()
        {

        }

        public QueryAuthAttribute(string[] roles)
        {
            Roles = roles;
        }

        public QueryAuthAttribute(string function)
        {
            FunctionName = function;
        }
    }
}
