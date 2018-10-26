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
                        await websocketConnection.Websocket.Send(new ExecuteResponse()
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
                            await websocketConnection.Websocket.Send(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = new Exception("User is not allowed to execute action.")
                            });

                            return;
                        }

                        ActionHandlerBase actionHandler = actionHandlerAccesor.GetActionHandler(actionHandlerType);
                        actionHandler.WebsocketConnection = websocketConnection;
                        actionHandler.ExecuteCommand = command;

                        if (actionHandler != null)
                        {
                            object result = actionMethod.Invoke(actionHandler, command.Parameters);

                            await websocketConnection.Websocket.Send(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Result = result
                            });

                            return;
                        }
                    }
                }

                await websocketConnection.Websocket.Send(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("No action to execute was found.")
                });
            }
            catch (Exception ex)
            {
                
                await websocketConnection.Websocket.Send(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = ex
                });
            }
        }
    }
}
