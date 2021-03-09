using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteEventAttribute : ModelStoreEventAttributeBase
    {
        public DeleteEventAttribute(string before = null, string beforeSave = null, string after = null, string insteadOf = null) : base(before, beforeSave, after, insteadOf)
        {

        }
    }
}
