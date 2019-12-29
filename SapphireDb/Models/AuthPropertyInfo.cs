using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class AuthPropertyInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public QueryAuthAttribute QueryAuthAttribute { get; set; }

        public UpdateAuthAttribute UpdateAuthAttribute { get; set; }

        public UpdatableAttribute UpdatableAttribute { get; set; }

        public NonCreatableAttribute NonCreatableAttribute { get; set; }

        public AuthPropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            QueryAuthAttribute = PropertyInfo.GetCustomAttribute<QueryAuthAttribute>();
            UpdateAuthAttribute = PropertyInfo.GetCustomAttribute<UpdateAuthAttribute>();
            UpdatableAttribute = PropertyInfo.GetCustomAttribute<UpdatableAttribute>();
            NonCreatableAttribute = PropertyInfo.GetCustomAttribute<NonCreatableAttribute>();
        }
    }
}
