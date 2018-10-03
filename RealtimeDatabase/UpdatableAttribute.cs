using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class UpdatableAttribute : Attribute
    {
    }
}
