using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class ModelAttributesInfo
    {
        public List<QueryAuthAttribute> QueryAuthAttributes { get; }
        
        public List<QueryEntryAuthAttribute> QueryEntryAuthAttributes { get; }

        public List<UpdateAuthAttribute> UpdateAuthAttributes { get; }
        
        public List<RemoveAuthAttribute> RemoveAuthAttributes { get; }
        
        public List<CreateAuthAttribute> CreateAuthAttributes { get; }

        public List<CreateEventAttribute> CreateEventAttributes { get; }
        
        public List<UpdateEventAttribute> UpdateEventAttributes { get; }
        
        public List<RemoveEventAttribute> RemoveEventAttributes { get; }

        public List<QueryAttribute> QueryAttributes { get; }
        
        public UpdateableAttribute UpdateableAttribute { get; set; }
        
        public DisableAutoMergeAttribute DisableAutoMergeAttribute { get; set; }
        
        public DisableCreateAttribute DisableCreateAttribute { get; set; }
        
        public DisableUpdateAttribute DisableUpdateAttribute { get; set; }
        
        public DisableDeleteAttribute DisableDeleteAttribute { get; set; }
        
        public DisableQueryAttribute DisableQueryAttribute { get; set; }
        
        public ModelAttributesInfo(Type modelType)
        {
            QueryAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<QueryAuthAttribute>(modelType);
            QueryEntryAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<QueryEntryAuthAttribute>(modelType);
            UpdateAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<UpdateAuthAttribute>(modelType);
            RemoveAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<RemoveAuthAttribute>(modelType);
            CreateAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<CreateAuthAttribute>(modelType);
            
            CreateEventAttributes = GetHookAttributeOfClassAndTopClasses<CreateEventAttribute>(modelType);
            UpdateEventAttributes = GetHookAttributeOfClassAndTopClasses<UpdateEventAttribute>(modelType);
            RemoveEventAttributes = GetHookAttributeOfClassAndTopClasses<RemoveEventAttribute>(modelType);

            QueryAttributes = GetQueryAttributes(modelType);
            UpdateableAttribute = modelType.GetCustomAttribute<UpdateableAttribute>(false);
            DisableAutoMergeAttribute = modelType.GetCustomAttribute<DisableAutoMergeAttribute>(false);
            
            DisableCreateAttribute = modelType.GetCustomAttribute<DisableCreateAttribute>(true);
            DisableUpdateAttribute = modelType.GetCustomAttribute<DisableUpdateAttribute>(true);
            DisableDeleteAttribute = modelType.GetCustomAttribute<DisableDeleteAttribute>(true);
            DisableQueryAttribute = modelType.GetCustomAttribute<DisableQueryAttribute>(true);
        }

        private List<QueryAttribute> GetQueryAttributes(Type modelType)
        {
            List<QueryAttribute> queryAttributes = modelType.GetCustomAttributes<QueryAttribute>(false).ToList();
            
            queryAttributes.ForEach(queryAttribute => queryAttribute.Compile(modelType));

            return queryAttributes;
        }
        
        private List<T> GetAuthAttributesOfClassOrDirectTopClass<T>(Type modelType) where T : AuthAttributeBase
        {
            Type currentModelType = modelType;
            List<T> attributes;

            do
            {
                attributes = currentModelType.GetCustomAttributes<T>(false).ToList();
                currentModelType = currentModelType.BaseType;
            } while (!attributes.Any() && currentModelType != null && currentModelType != typeof(object));

            attributes.ForEach(attribute =>
            {
                attribute.Compile(modelType, AuthAttributeBase.CompileContext.Class);
            });
            
            return attributes;
        }

        private List<T> GetHookAttributeOfClassAndTopClasses<T>(Type modelType) where T : ModelStoreEventAttributeBase
        {
            List<T> hookAttributes = new List<T>();
            Type currentType = modelType;
            
            while (currentType != null && currentType != typeof(object))
            {
                foreach (T attribute in currentType.GetCustomAttributes<T>(false))
                {
                    attribute.Compile(currentType);
                    hookAttributes.Add(attribute);
                }

                currentType = currentType.BaseType;
            }

            return hookAttributes;
        }
    }
}