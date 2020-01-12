using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class PropertyAttributesInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public List<QueryAuthAttribute> QueryAuthAttributes { get; set; }

        public List<UpdateAuthAttribute> UpdateAuthAttributes { get; set; }

        public UpdatableAttribute UpdatableAttribute { get; set; }

        public NonCreatableAttribute NonCreatableAttribute { get; set; }

        public PropertyAttributesInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            QueryAuthAttributes = GetCustomAttributes<QueryAuthAttribute>();
            UpdateAuthAttributes = GetCustomAttributes<UpdateAuthAttribute>();
            UpdatableAttribute = PropertyInfo.GetCustomAttribute<UpdatableAttribute>(false);
            NonCreatableAttribute = PropertyInfo.GetCustomAttribute<NonCreatableAttribute>(false);
        }

        private List<T> GetCustomAttributes<T>() where T : AuthAttributeBase
        {
            List<T> attributes = PropertyInfo.GetCustomAttributes<T>(false).ToList();
            
            attributes.ForEach(attribute =>
            {
                attribute.Compile(PropertyInfo.DeclaringType);
            });
            
            return attributes;
        }
    }
}
