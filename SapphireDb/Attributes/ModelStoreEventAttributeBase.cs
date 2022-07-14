using System;
using System.Reflection;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Attributes
{
    public abstract class ModelStoreEventAttributeBase : Attribute, ICompilableAttribute
    {
        public string Before { get; set; }

        public MethodInfo BeforeFunction { get; set; }
        
        public Action<object, object, IConnectionInformation> BeforeLambda { get; set; }
        
        public string BeforeSave { get; set; }

        public MethodInfo BeforeSaveFunction { get; set; }
        
        public Action<object, object, IConnectionInformation> BeforeSaveLambda { get; set; }
        
        public string After { get; set; }
        
        public MethodInfo AfterFunction { get; set; }
        
        public Action<object, object, IConnectionInformation> AfterLambda { get; set; }
        
        public string InsteadOf { get; set; }
        
        public MethodInfo InsteadOfFunction { get; set; }
        
        public Action<object, object, IConnectionInformation> InsteadOfLambda { get; set; }

        public ModelStoreEventAttributeBase(string before = null, string beforeSave = null, string after = null, string insteadOf = null)
        {
            Before = before;
            BeforeSave = beforeSave;
            After = after;
            InsteadOf = insteadOf;
        }

        public void Compile(Type declaredType, Type modelType = null)
        {
            BeforeFunction = ReflectionMethodHelper.GetMethodInfo(declaredType, Before, typeof(void));
            BeforeSaveFunction = ReflectionMethodHelper.GetMethodInfo(declaredType, BeforeSave, typeof(void));
            AfterFunction = ReflectionMethodHelper.GetMethodInfo(declaredType, After, typeof(void));
            InsteadOfFunction = ReflectionMethodHelper.GetMethodInfo(declaredType, InsteadOf);
        }

        public enum EventType
        {
            Before, BeforeSave, After, InsteadOf
        }
    }
}
