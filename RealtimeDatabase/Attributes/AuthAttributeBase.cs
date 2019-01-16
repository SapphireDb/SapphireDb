using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Attributes
{
    public class AuthAttributeBase : Attribute
    {
        public string[] Roles { get; set; }

        public string FunctionName { get; set; }

        public AuthAttributeBase()
        {

        }

        public AuthAttributeBase(string[] roles)
        {
            Roles = roles;
        }

        public AuthAttributeBase(string function)
        {
            FunctionName = function;
        }
    }
}
