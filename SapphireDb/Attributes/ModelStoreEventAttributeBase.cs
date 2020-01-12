using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace SapphireDb.Attributes
{
    public class ModelStoreEventAttributeBase : Attribute
    {
        public string Before { get; set; }

        public MethodInfo BeforeFunction { get; set; }
        
        public string BeforeSave { get; set; }

        public MethodInfo BeforeSaveFunction { get; set; }
        
        public string After { get; set; }
        
        public MethodInfo AfterFunction { get; set; }

        public ModelStoreEventAttributeBase(string before = null, string beforeSave = null, string after = null)
        {
            Before = before;
            BeforeSave = beforeSave;
            After = after;
        }

        public void Compile(Type modelType)
        {
            BeforeFunction = GetMethodInfo(modelType, Before);
            BeforeSaveFunction = GetMethodInfo(modelType, BeforeSave);
            AfterFunction = GetMethodInfo(modelType, After);
        }

        private MethodInfo GetMethodInfo(Type modelType, string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }
            
            MethodInfo methodInfo = modelType.GetMethod(methodName,
                BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (methodInfo == null || methodInfo.ReturnType != typeof(void))
            {
                throw new Exception("No suiting method was found");
            }

            return methodInfo;
        }
    }
}
