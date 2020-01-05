using System;
using System.Collections.Generic;
using System.Text;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RemoveEventAttribute : ModelStoreEventAttributeBase
    {
        public RemoveEventAttribute(string before = null, string beforeSave = null, string after = null) : base(before, beforeSave, after)
        {

        }
    }
}
