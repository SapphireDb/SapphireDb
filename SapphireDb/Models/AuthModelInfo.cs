using System;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class AuthModelInfo
    {
        public QueryAuthAttribute QueryAuthAttribute { get; set; }

        public UpdateAuthAttribute UpdateAuthAttribute { get; set; }
        
        public RemoveAuthAttribute RemoveAuthAttribute { get; set; }
        
        public CreateAuthAttribute CreateAuthAttribute { get; set; }
        
        public AuthModelInfo(Type modelType)
        {
            QueryAuthAttribute = modelType.GetCustomAttribute<QueryAuthAttribute>();
            UpdateAuthAttribute = modelType.GetCustomAttribute<UpdateAuthAttribute>();
            RemoveAuthAttribute = modelType.GetCustomAttribute<RemoveAuthAttribute>();
            CreateAuthAttribute = modelType.GetCustomAttribute<CreateAuthAttribute>();
        }
    }
}