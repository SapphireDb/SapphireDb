using System;
using System.Reflection;
using SapphireDb.Actions;
using SapphireDb.Attributes;
using SapphireDb.Connection;
using SapphireDb.Helper;

namespace SapphireDb.Models.SapphireApiBuilder
{
    public class SapphireActionHandlerBuilder<T>
        where T : ActionHandlerBase
    {
        private readonly ActionHandlerAttributesInfo attributesInfo;
        
        public SapphireActionHandlerBuilder()
        {
             attributesInfo = typeof(T).GetActionHandlerAttributesInfo();
        }

        public SapphireActionHandlerBuilder<T> AddActionAuth(string policies = null,
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

        public SapphireActionBuilder Action(string actionName)
        {
            MethodInfo method = typeof(T).GetMethod(actionName,
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly |
                BindingFlags.IgnoreCase);

            return method != null ? new SapphireActionBuilder(method) : null;
        }
    }
}