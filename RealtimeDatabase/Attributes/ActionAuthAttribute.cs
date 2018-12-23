using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ActionAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public string FunctionName { get; set; }

        public ActionAuthAttribute()
        {

        }

        public ActionAuthAttribute(string[] roles)
        {
            Roles = roles;
        }

        public ActionAuthAttribute(string function)
        {
            FunctionName = function;
        }
    }
}
