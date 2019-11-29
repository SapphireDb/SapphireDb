using System;
using System.Collections.Generic;
using System.Text;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelStoreEventAttributeBase : Attribute
    {
        public string Before { get; set; }

        public string BeforeSave { get; set; }

        public string After { get; set; }

        public ModelStoreEventAttributeBase(string before = null, string beforeSave = null, string after = null)
        {
            Before = before;
            BeforeSave = beforeSave;
            After = after;
        }
    }
}
