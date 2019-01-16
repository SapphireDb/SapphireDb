using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ActionAuthAttribute : AuthAttributeBase
    {
        public ActionAuthAttribute()
        {
        }

        public ActionAuthAttribute(string[] roles) : base(roles)
        {
        }

        public ActionAuthAttribute(string function) : base(function)
        {
        }
    }
}
