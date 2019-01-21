using System;

namespace RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class UpdateAuthAttribute : AuthAttributeBase
    {
        public UpdateAuthAttribute()
        {
            
        }

        public UpdateAuthAttribute(string[] roles) : base(roles)
        {
        }

        public UpdateAuthAttribute(string function) : base(function)
        {
        }
    }
}
