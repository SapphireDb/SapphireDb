using System;

namespace SapphireDb.Attributes
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
