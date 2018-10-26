using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Internal
{
    class ActionMapper
    {
        private readonly ActionHandlerInformation[] actions;

        public ActionMapper(ActionHandlerInformation[] _actions)
        {
            actions = _actions;
        }

        public Type GetHandler(ExecuteCommand executeCommand)
        {
            ActionHandlerInformation actionHandlerInformation = actions.FirstOrDefault(a => a.Name == executeCommand.ActionHandlerName);

            if (actionHandlerInformation != null)
            {
                return actionHandlerInformation.Type;
            }

            return null;
        }

        public MethodInfo GetAction(ExecuteCommand executeCommand, Type actionHandlerType)
        {
            return actionHandlerType.GetMethod(executeCommand.ActionName);
        }
    }
}
