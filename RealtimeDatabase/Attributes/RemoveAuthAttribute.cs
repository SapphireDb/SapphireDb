using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoveAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public string FunctionName { get; set; }

        public RemoveAuthAttribute()
        {

        }

        public RemoveAuthAttribute(string[] roles)
        {
            Roles = roles;
        }

        public RemoveAuthAttribute(string function)
        {
            FunctionName = function;
        }
    }
}
