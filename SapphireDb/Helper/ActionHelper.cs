using System;
using System.Collections.Concurrent;
using System.Reflection;
using SapphireDb.Models;

namespace SapphireDb.Helper
{
    static class ActionHelper
    {
        private static readonly ConcurrentDictionary<Type, ActionHandlerAttributesInfo> ActionHandlerAttributesInfos = new ConcurrentDictionary<Type, ActionHandlerAttributesInfo>();

        public static ActionHandlerAttributesInfo GetActionHandlerAttributesInfo(this Type actionHandlerType)
        {
            if (ActionHandlerAttributesInfos.TryGetValue(actionHandlerType, out ActionHandlerAttributesInfo authActionHandlerInfo))
            {
                return authActionHandlerInfo;
            }

            authActionHandlerInfo = new ActionHandlerAttributesInfo(actionHandlerType);
            ActionHandlerAttributesInfos.TryAdd(actionHandlerType, authActionHandlerInfo);

            return authActionHandlerInfo;
        }
        
        private static readonly ConcurrentDictionary<MethodInfo, ActionAttributesInfo> ActionAttributesInfos = new ConcurrentDictionary<MethodInfo, ActionAttributesInfo>();

        public static ActionAttributesInfo GetActionAttributesInfo(this MethodInfo action)
        {
            if (ActionAttributesInfos.TryGetValue(action, out ActionAttributesInfo authActionInfo))
            {
                return authActionInfo;
            }

            authActionInfo = new ActionAttributesInfo(action);
            ActionAttributesInfos.TryAdd(action, authActionInfo);

            return authActionInfo;
        }
    }
}