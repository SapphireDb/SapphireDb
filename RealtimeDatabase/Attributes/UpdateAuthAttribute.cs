using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class UpdateAuthAttribute : Attribute
    {
        public string[] Roles { get; set; }

        public string FunctionName { get; set; }

        public UpdateAuthAttribute()
        {
            
        }

        public UpdateAuthAttribute(string[] roles)
        {
            Roles = roles;
        }

        public UpdateAuthAttribute(string function)
        {
            FunctionName = function;
        }
    }
}
