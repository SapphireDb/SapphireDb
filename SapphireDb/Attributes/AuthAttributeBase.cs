using System;

namespace SapphireDb.Attributes
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
