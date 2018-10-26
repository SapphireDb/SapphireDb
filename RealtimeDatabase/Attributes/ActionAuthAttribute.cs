using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ActionAuthAttribute : Attribute
    {
        public string[] Roles;

        public ActionAuthAttribute()
        {

        }

        public ActionAuthAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
