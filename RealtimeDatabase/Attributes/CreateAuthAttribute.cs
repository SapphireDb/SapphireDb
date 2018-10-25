using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public CreateAuthAttribute()
        {

        }

        public CreateAuthAttribute(params string[] roles)
        {
            Roles = roles;
        }
    }
}
