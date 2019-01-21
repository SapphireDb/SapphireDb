using System;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoveAuthAttribute : AuthAttributeBase
    {
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
