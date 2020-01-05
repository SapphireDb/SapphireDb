using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class AuthPropertyInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public List<QueryAuthAttribute> QueryAuthAttributes { get; set; }

        public List<UpdateAuthAttribute> UpdateAuthAttributes { get; set; }

        public UpdatableAttribute UpdatableAttribute { get; set; }

        public NonCreatableAttribute NonCreatableAttribute { get; set; }

        public AuthPropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            QueryAuthAttributes = PropertyInfo.GetCustomAttributes<QueryAuthAttribute>(false).ToList();
            UpdateAuthAttributes = PropertyInfo.GetCustomAttributes<UpdateAuthAttribute>(false).ToList();
            UpdatableAttribute = PropertyInfo.GetCustomAttribute<UpdatableAttribute>(false);
            NonCreatableAttribute = PropertyInfo.GetCustomAttribute<NonCreatableAttribute>(false);
        }
    }
}
