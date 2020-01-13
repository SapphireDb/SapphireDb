using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SapphireDb.Actions;
using SapphireDb.Command.Execute;
using SapphireDb.Helper;

namespace SapphireDb.Internal
{
    class ActionMapper
    {
        public readonly Dictionary<string, Type> actionHandlerTypes;
        private readonly Dictionary<Type, MethodInfo[]> actionHandlerActions;

        public ActionMapper()
        {
            actionHandlerTypes = Assembly.GetEntryAssembly()?.GetTypes()
                .Where(t => typeof(ActionHandlerBase).IsAssignableFrom(t) && t.Name.EndsWith("Actions"))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Actions", StringComparison.Ordinal)).ToCamelCase(), t => t);

            actionHandlerActions = actionHandlerTypes?.ToDictionary(t => t.Value,
                t => t.Value.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly));
        }

        public Type GetHandler(ExecuteCommand executeCommand)
        {
            return actionHandlerTypes[executeCommand.ActionHandlerName.ToCamelCase()];
        }

        public MethodInfo GetAction(ExecuteCommand executeCommand, Type actionHandlerType)
        {
            if (actionHandlerActions.TryGetValue(actionHandlerType, out MethodInfo[] actions))
            {
                return actions.FirstOrDefault(a =>
                    a.Name.Equals(executeCommand.ActionName, StringComparison.InvariantCultureIgnoreCase));
            }

            return null;
        }
    }
}
