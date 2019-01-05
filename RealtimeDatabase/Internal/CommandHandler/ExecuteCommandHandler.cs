using Microsoft.AspNetCore.Http;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
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
                Type actionHandlerType = actionMapper.GetHandler(command);

                if (actionHandlerType != null)
                {
                    MethodInfo actionMethod = actionMapper.GetAction(command, actionHandlerType);

                    if (actionMethod != null)
                    {
                        ActionHandlerBase actionHandler = (ActionHandlerBase)serviceProvider.GetService(actionHandlerType);

                        if (!actionHandlerType.CanExecuteAction(websocketConnection, actionHandler))
                        {
                            await websocketConnection.Send(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = new Exception("User is not allowed to execute actions of this handler.")
                            });

                            return;
                        }

                        if (!actionMethod.CanExecuteAction(websocketConnection, actionHandler))
                        {
                            await websocketConnection.Send(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Error = new Exception("User is not allowed to execute action.")
                            });

                            return;
                        }

                        if (actionHandler != null)
                        {
                            actionHandler.WebsocketConnection = websocketConnection;
                            actionHandler.ExecuteCommand = command;

                            object[] parameters = actionMethod.GetParameters().Select(parameter => {
                                object parameterValue = command.Parameters[parameter.Position];

                                if (parameterValue.GetType() == typeof(JObject))
                                {
                                    return ((JObject)parameterValue).ToObject(parameter.ParameterType);
                                }

                                return parameterValue;
                            }).ToArray();

                            object result = actionMethod.Invoke(actionHandler, parameters);

                            if (result != null)
                            {
                                Type type = result.GetType();

                                if (type.BaseType.BaseType == typeof(Task))
                                {
                                    result = await (dynamic)result;
                                }
                                else if (type.BaseType == typeof(Task))
                                {
                                    await (dynamic)result;
                                    result = null;
                                }
                            }

                            await websocketConnection.Send(new ExecuteResponse()
                            {
                                ReferenceId = command.ReferenceId,
                                Result = result
                            });

                            logger.LogInformation("Executed " + actionMethod.Name + " in " + actionMethod.DeclaringType.Name);

                            return;
                        }
                    }
                }

                await websocketConnection.Send(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Error = new Exception("No action to execute was found.")
                });
            }
            catch (RuntimeBinderException)
            {
                await websocketConnection.Send(new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Result = null
                });
            }
            //catch (Exception ex)
            //{
            //    await websocketConnection.Send(new ExecuteResponse()
            //    {
            //        ReferenceId = command.ReferenceId,
            //        Error = ex
            //    });
            //}
        }
    }
}
