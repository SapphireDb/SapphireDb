using System;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NonCreatableAttribute : Attribute
    {
    }
}
