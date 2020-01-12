using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    public class ActionHandlerAttributesInfo
    {
        public List<ActionAuthAttribute> ActionAuthAttributes { get; set; }
        
        public ActionHandlerAttributesInfo(Type actionHandlerType)
        {
            ActionAuthAttributes = GetCustomAttributes<ActionAuthAttribute>(actionHandlerType);
        }

        private List<T> GetCustomAttributes<T>(Type actionHandlerType) where T : AuthAttributeBase
        {
            List<T> attributes = actionHandlerType.GetCustomAttributes<T>(false).ToList();
            
            attributes.ForEach(attribute =>
            {
                attribute.Compile(actionHandlerType);
            });
            
            return attributes;
        }
    }
}