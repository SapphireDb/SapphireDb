using System;

namespace SapphireDb.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class TypeDiscriminatorAttribute : Attribute
{
    public string TypeName { get; }
    public Type Type { get; }

    public TypeDiscriminatorAttribute(string typeName, Type type)
    {
        TypeName = typeName;
        Type = type;
    }
}