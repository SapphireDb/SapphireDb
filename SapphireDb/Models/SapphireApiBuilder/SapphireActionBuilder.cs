using System;
using System.Reflection;
using SapphireDb.Attributes;
using SapphireDb.Connection;
using SapphireDb.Helper;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireActionBuilder
    {
        private readonly ActionAttributesInfo attributesInfo;
        
        public SapphireActionBuilder(MethodInfo methodInfo)
        {
            attributesInfo = methodInfo.GetActionAttributesInfo();
        }
        
        public SapphireActionBuilder AddActionAuth(string policies = null,
            Func<IConnectionInformation, bool> function = null)
        {
            ActionAuthAttribute attribute = new ActionAuthAttribute(policies);

            if (function != null)
            {
                attribute.FunctionLambda = (information, _) => function(information);
            }
            
            attributesInfo.ActionAuthAttributes.Add(attribute);
            
            return this;
        }
    }
}