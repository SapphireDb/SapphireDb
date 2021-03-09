using System;
using System.Reflection;
using SapphireDb.Helper;
using SapphireDb.Models;

namespace SapphireDb.Attributes
{
    public class ModelStoreEventAttributeBase : Attribute
    {
        public string Before { get; set; }

        public MethodInfo BeforeFunction { get; set; }
        
        public Action<object, object, HttpInformation> BeforeLambda { get; set; }
        
        public string BeforeSave { get; set; }

        public MethodInfo BeforeSaveFunction { get; set; }
        
        public Action<object, object, HttpInformation> BeforeSaveLambda { get; set; }
        
        public string After { get; set; }
        
        public MethodInfo AfterFunction { get; set; }
        
        public Action<object, object, HttpInformation> AfterLambda { get; set; }
        
        public string InsteadOf { get; set; }
        
        public MethodInfo InsteadOfFunction { get; set; }
        
        public Action<object, object, HttpInformation> InsteadOfLambda { get; set; }

        public ModelStoreEventAttributeBase(string before = null, string beforeSave = null, string after = null, string insteadOf = null)
        {
            Before = before;
            BeforeSave = beforeSave;
            After = after;
            InsteadOf = insteadOf;
        }

        public void Compile(Type modelType)
        {
            BeforeFunction = ReflectionMethodHelper.GetMethodInfo(modelType, Before, typeof(void));
            BeforeSaveFunction = ReflectionMethodHelper.GetMethodInfo(modelType, BeforeSave, typeof(void));
            AfterFunction = ReflectionMethodHelper.GetMethodInfo(modelType, After, typeof(void));
            InsteadOfFunction = ReflectionMethodHelper.GetMethodInfo(modelType, InsteadOf, typeof(void));
        }

        public enum EventType
        {
            Before, BeforeSave, After, InsteadOf
        }
    }
}
