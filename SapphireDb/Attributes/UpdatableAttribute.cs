using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class UpdatableAttribute : Attribute
    {
    }
}
