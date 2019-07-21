using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using RealtimeDatabase.Attributes;

namespace RealtimeDatabase.Models.Auth
{
    public class AuthPropertyInfo
    {
        public PropertyInfo PropertyInfo { get; set; }

        public QueryAuthAttribute QueryAuthAttribute { get; set; }

        public UpdateAuthAttribute UpdateAuthAttribute { get; set; }

        public UpdatableAttribute UpdatableAttribute { get; set; }


        public AuthPropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            QueryAuthAttribute = PropertyInfo.GetCustomAttribute<QueryAuthAttribute>();
            UpdateAuthAttribute = PropertyInfo.GetCustomAttribute<UpdateAuthAttribute>();
            UpdatableAttribute = PropertyInfo.GetCustomAttribute<UpdatableAttribute>();
        }
    }
}
