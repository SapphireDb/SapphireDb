using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class UpdateEventAttribute : ModelStoreEventAttributeBase
    {
        public UpdateEventAttribute(string before = null, string beforeSave = null, string after = null, string insteadOf = null) : base(before, beforeSave, after, insteadOf)
        {

        }
    }
}
