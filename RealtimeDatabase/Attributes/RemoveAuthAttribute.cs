using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoveAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public RemoveAuthAttribute()
        {

        }

        public RemoveAuthAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
