using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class ExecuteCommandHandler : CommandHandlerBase, ICommandHandler<ExecuteCommand>
    {
        private readonly ActionMapper actionMapper;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ExecuteCommandHandler> logger;

        public ExecuteCommandHandler(DbContextAccesor contextAccesor, ActionMapper _actionMapper, IServiceProvider _serviceProvider, ILogger<ExecuteCommandHandler> logger)
            : base(contextAccesor)
        {
            actionMapper = _actionMapper;
            serviceProvider = _serviceProvider;
            this.logger = logger;
        }

        public async Task Handle(WebsocketConnection websocketConnection, ExecuteCommand command)
        {
            try
            {
                await GetActionDetails(command, websocketConnection);
            }
            catch (RuntimeBinderException)
            {
                await websocketConnection.Send(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Result = null
                });
            }
        }

        private async Task GetActionDetails(ExecuteCommand command, WebsocketConnection websocketConnection)
        {
            Type actionHandlerType = actionMapper.GetHandler(command);

            if (actionHandlerType != null)
            {
                MethodInfo actionMethod = actionMapper.GetAction(command, actionHandlerType);

                if (actionMethod != null)
                {
                    ActionHandlerBase actionHandler = (ActionHandlerBase)serviceProvider.GetService(actionHandlerType);

                    if (actionHandler != null)
                    {
                        if (!await CheckAccessToHandler(actionHandlerType, actionMethod, command, actionHandler, websocketConnection))
                            return;

                        await ExecuteAction(actionHandler, websocketConnection, command, actionMethod);
                    }
                }
                else
                {
                    await websocketConnection.SendException<ExecuteResponse>(command,
                        "No action to execute was found.");
                }
            }
        }

        private async Task ExecuteAction(ActionHandlerBase actionHandler, WebsocketConnection websocketConnection, ExecuteCommand command,
            MethodInfo actionMethod)
        {
            actionHandler.WebsocketConnection = websocketConnection;
            actionHandler.ExecuteCommand = command;

            logger.LogInformation("Execution of " + actionMethod.DeclaringType.FullName + "." + actionMethod.Name + " started");

            object result = actionMethod.Invoke(actionHandler, GetParameters(actionMethod, command));

            if (result != null)
            {
                try { result = await (dynamic)result; }
                catch { /* Do nothing because result is not an awaitable and already contains the expected result */ }
            }

            await websocketConnection.Send(new ExecuteResponse()
            {
                ReferenceId = command.ReferenceId,
                Result = result
            });

            logger.LogInformation("Executed " + actionMethod.DeclaringType.FullName + "." + actionMethod.Name);
        }

        private async Task<bool> CheckAccessToHandler(Type actionHandlerType, MethodInfo actionMethod, ExecuteCommand command,
            ActionHandlerBase actionHandler, WebsocketConnection websocketConnection)
        {
            if (!actionHandlerType.CanExecuteAction(websocketConnection, actionHandler))
            {
                await websocketConnection.SendException<ExecuteResponse>(command,
                    "User is not allowed to execute actions of this handler.");
                return false;
            }

            if (!actionMethod.CanExecuteAction(websocketConnection, actionHandler))
            {
                await websocketConnection.SendException<ExecuteResponse>(command,
                    "User is not allowed to execute action.");
                return false;
            }

            return true;
        }

        private object[] GetParameters(MethodInfo actionMethod, ExecuteCommand command)
        {
            return actionMethod.GetParameters().Select(parameter => {
                object parameterValue = command.Parameters[parameter.Position];

                if (parameterValue.GetType() == typeof(JObject))
                {
                    return ((JObject)parameterValue).ToObject(parameter.ParameterType);
                }

                return parameterValue;
            }).ToArray();
        }
    }
}
