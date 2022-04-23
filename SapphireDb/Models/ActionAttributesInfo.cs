using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    class ActionAttributesInfo
    {
        public MethodInfo MethodInfo { get; set; }
        
        public List<ActionAuthAttribute> ActionAuthAttributes { get; set; }
        
        public ActionAttributesInfo(MethodInfo action)
        {
            MethodInfo = action;
            ActionAuthAttributes = GetCustomAttributes<ActionAuthAttribute>();
        }

        private List<T> GetCustomAttributes<T>() where T : AuthAttributeBase
        {
            List<T> attributes = MethodInfo.GetCustomAttributes<T>(false).ToList();
            
            attributes.ForEach(attribute =>
            {
                attribute.Compile(MethodInfo.DeclaringType, AuthAttributeBase.CompileContext.Method);
            });
            
            return attributes;
        }
    }
}