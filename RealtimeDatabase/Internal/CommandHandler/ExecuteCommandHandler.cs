using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class ExecuteCommandHandler : CommandHandlerBase, ICommandHandler<ExecuteCommand>
    {
        private readonly IServiceProvider serviceProvider;

        public ExecuteCommandHandler(DbContextAccesor contextAccesor, WebsocketConnection websocketConnection, IServiceProvider _serviceProvider)
            : base(contextAccesor, websocketConnection)
        {
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
                    MethodInfo actionMethod = actionMapper.GetAction(command, actionHandlerType);

                    if (actionMethod != null)
                    {
                        ActionHandlerBase actionHandler = (ActionHandlerBase)serviceProvider.GetService(actionHandlerType);
                        actionHandler.WebsocketConnection = websocketConnection;

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
