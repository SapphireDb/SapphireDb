using System;
using System.Reflection;
using SapphireDb.Helper;
using SapphireDb.Models.Exceptions;
using SapphireDb.Models.SapphireApiBuilder;

namespace SapphireDb.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DefaultQueryAttribute : QueryAttributeBase, ICompilableAttribute
    {
        public DefaultQueryAttribute(string functionName)
        {
            FunctionName = functionName;
        }
        
        public void Compile(Type declaredType, Type modelType)
        {
            MethodInfo functionInfo = ReflectionMethodHelper.GetMethodInfo(declaredType, FunctionName, null,
                BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            Type queryBuilderType = typeof(SapphireQueryBuilder<>).MakeGenericType(modelType);
            
            if (!functionInfo.IsGenericMethod)
            {
                throw new WrongReturnTypeException(declaredType.Name, FunctionName, queryBuilderType.Name);
            }

            MethodInfo functionInfoGeneric = functionInfo.MakeGenericMethod(modelType);

            if (!queryBuilderType.IsAssignableFrom(functionInfoGeneric.ReturnType))
            {
                throw new WrongReturnTypeException(declaredType.Name, FunctionName, queryBuilderType.Name);
            }

            FunctionInfo = functionInfoGeneric;
        }
    }
}