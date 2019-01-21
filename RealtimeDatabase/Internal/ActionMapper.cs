using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RealtimeDatabase.Internal
{
    class ActionMapper
    {
        public readonly Dictionary<string, Type> actionHandlerTypes;

        public ActionMapper()
        {
            actionHandlerTypes = Assembly.GetEntryAssembly().GetTypes()
                .Where(t => typeof(ActionHandlerBase).IsAssignableFrom(t) && t.Name.EndsWith("Actions"))
                .ToDictionary(t => t.Name.Substring(0, t.Name.LastIndexOf("Actions")).ToCamelCase(), t => t);
        }

        public Type GetHandler(ExecuteCommand executeCommand)
        {
            return actionHandlerTypes[executeCommand.ActionHandlerName.ToCamelCase()];
        }

        public MethodInfo GetAction(ExecuteCommand executeCommand, Type actionHandlerType)
        {
            return actionHandlerType.GetMethods().FirstOrDefault(m => m.Name.ToCamelCase() == executeCommand.ActionName.ToCamelCase());
        }
    }
}
