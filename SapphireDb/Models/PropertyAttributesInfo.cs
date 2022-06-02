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

        public UpdateableAttribute UpdateableAttribute { get; set; }

        public NonCreatableAttribute NonCreatableAttribute { get; set; }
        
        public MergeConflictResolutionModeAttribute MergeConflictResolutionModeAttribute { get; set; }

        public ConcealAttribute ConcealAttribute { get; set; }

        public ExposeAttribute ExposeAttribute { get; set; }

        public PropertyAttributesInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            QueryAuthAttributes = GetCustomAttributes<QueryAuthAttribute>();
            UpdateAuthAttributes = GetCustomAttributes<UpdateAuthAttribute>();
            UpdateableAttribute = PropertyInfo.GetCustomAttribute<UpdateableAttribute>(false);
            NonCreatableAttribute = PropertyInfo.GetCustomAttribute<NonCreatableAttribute>(false);
            MergeConflictResolutionModeAttribute = PropertyInfo.GetCustomAttribute<MergeConflictResolutionModeAttribute>();
            ConcealAttribute = PropertyInfo.GetCustomAttribute<ConcealAttribute>();
            ExposeAttribute = PropertyInfo.GetCustomAttribute<ExposeAttribute>();
        }

        private List<T> GetCustomAttributes<T>() where T : AuthAttributeBase
        {
            List<T> attributes = PropertyInfo.GetCustomAttributes<T>(false).ToList();
            
            attributes.ForEach(attribute =>
            {
                attribute.Compile(PropertyInfo.DeclaringType, AuthAttributeBase.CompileContext.Property);
            });
            
            return attributes;
        }
    }
}
