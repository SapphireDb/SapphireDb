using System;

namespace SapphireDb.Attributes
{
    public interface ICompilableAttribute
    {
        void Compile(Type declaredType, Type modelType = null);
    }
}