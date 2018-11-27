using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class ExecuteCommandHandler : CommandHandlerBase, ICommandHandler<ExecuteCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ActionHandlerAccesor actionHandlerAccesor;

        public ExecuteCommandHandler(DbContextAccesor contextAccesor, WebsocketConnection websocketConnection,
            ActionHandlerAccesor _actionHandlerAccesor, IServiceProvider _serviceProvider)
            : base(contextAccesor, websocketConnection)
        {
            actionHandlerAccesor = _actionHandlerAccesor;
            serviceProvider = _serviceProvider;
        }

        public async Task Handle(ExecuteCommand command)
        {
            try
            {
                ActionMapper actionMapper = (ActionMapper)serviceProvider.GetService(typeof(ActionMapper));

                Type actionHandlerType = actionMapper.GetHandler(command);

                if (actionHandlerType != null)
                {
                    if (!actionHandlerType.CanExecuteAction(websocketConnection))
                    {
                        await SendMessage(new ExecuteResponse()
                        {
                            ReferenceId = command.ReferenceId,
                            Error = new Exception("User is not allowed to execute action.")
                        });

                        return;
                    }

                    MethodInfo actionMethod = actionMapper.GetAction(command, actionHandlerType);

                    if (actionMethod != null)
                    {
                        if (!actionMethod.CanExecuteAction(websocketConnection))
                        {
                            await SendMessage(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = new Exception("User is not allowed to execute action.")
                            });

                            return;
                        }

                        ActionHandlerBase actionHandler = actionHandlerAccesor.GetActionHandler(actionHandlerType);

                        if (actionHandler != null)
                        {
                            actionHandler.WebsocketConnection = websocketConnection;
                            actionHandler.ExecuteCommand = command;

                            object result = actionMethod.Invoke(actionHandler, command.Parameters);

                            if (result != null && result.GetType().BaseType == typeof(Task))
                            {
                                result = result.GetType().GetProperty("Result").GetValue(result);
                            }

                            await SendMessage(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Result = result
                            });

                            return;
                        }
                    }
                }

                await SendMessage(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("No action to execute was found.")
                });
            }
            catch (Exception ex)
            {
                
                await SendMessage(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = ex
                });
            }
        }
    }
}
