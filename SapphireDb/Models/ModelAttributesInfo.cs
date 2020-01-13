using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class ModelAttributesInfo
    {
        public List<QueryAuthAttribute> QueryAuthAttributes { get; set; }
        
        public List<QueryEntryAuthAttribute> QueryEntryAuthAttributes { get; set; }

        public List<UpdateAuthAttribute> UpdateAuthAttributes { get; set; }
        
        public List<RemoveAuthAttribute> RemoveAuthAttributes { get; set; }
        
        public List<CreateAuthAttribute> CreateAuthAttributes { get; set; }

        public List<CreateEventAttribute> CreateEventAttributes { get; set; }
        
        public List<UpdateEventAttribute> UpdateEventAttributes { get; set; }
        
        public List<RemoveEventAttribute> RemoveEventAttributes { get; set; }
        
        public QueryFunctionAttribute QueryFunctionAttribute { get; set; }
        
        public UpdatableAttribute UpdatableAttribute { get; set; }
        
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

            QueryFunctionAttribute = modelType.GetCustomAttribute<QueryFunctionAttribute>(false);
            QueryFunctionAttribute?.Compile(modelType);

            UpdatableAttribute = modelType.GetCustomAttribute<UpdatableAttribute>(false);
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
                attribute.Compile(modelType);
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