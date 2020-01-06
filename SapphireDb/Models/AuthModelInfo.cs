using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class AuthModelInfo
    {
        public List<QueryAuthAttribute> QueryAuthAttributes { get; set; }
        
        public List<QueryEntryAuthAttribute> QueryEntryAuthAttributes { get; set; }

        public List<UpdateAuthAttribute> UpdateAuthAttributes { get; set; }
        
        public List<RemoveAuthAttribute> RemoveAuthAttributes { get; set; }
        
        public List<CreateAuthAttribute> CreateAuthAttributes { get; set; }
        
        public AuthModelInfo(Type modelType)
        {
            QueryAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<QueryAuthAttribute>(modelType);
            QueryEntryAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<QueryEntryAuthAttribute>(modelType);
            UpdateAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<UpdateAuthAttribute>(modelType);
            RemoveAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<RemoveAuthAttribute>(modelType);
            CreateAuthAttributes = GetAuthAttributesOfClassOrDirectTopClass<CreateAuthAttribute>(modelType);
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

            return attributes;
        }
    }
}