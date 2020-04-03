using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SapphireDb.Actions;
using SapphireDb.Connection;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.Execute
{
    class ExecuteCommandHandler : CommandHandlerBase, ICommandHandler<ExecuteCommand>, INeedsConnection
    {
        private readonly ActionMapper actionMapper;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ExecuteCommandHandler> logger;
        public ConnectionBase Connection { get; set; }

        public ExecuteCommandHandler(DbContextAccesor contextAccessor, ActionMapper actionMapper,
            IServiceProvider serviceProvider, ILogger<ExecuteCommandHandler> logger)
            : base(contextAccessor)
        {
            this.actionMapper = actionMapper;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, ExecuteCommand command)
        {
            try
            {
                return await GetActionDetails(command, context);
            }
            catch (RuntimeBinderException)
            {
                return new ExecuteResponse()
                {
                    ReferenceId = command.ReferenceId,
                    Result = null
                };
            }
        }

        private async Task<ResponseBase> GetActionDetails(ExecuteCommand command, HttpInformation context)
        {
            string[] actionParts = command?.Action.Split('.');

            if (actionParts == null || actionParts.Length != 2)
            {
                return command.CreateExceptionResponse<ExecuteResponse>(
                    new FormatException("Wrong format of action name."));
            }

            string actionHandlerName = actionParts[0];
            string actionName = actionParts[1];

            Type actionHandlerType = actionMapper.GetHandler(actionHandlerName);

            if (actionHandlerType != null)
            {
                MethodInfo actionMethod = actionMapper.GetAction(actionName, actionHandlerType);

                if (actionMethod != null)
                {
                    ActionHandlerBase actionHandler = (ActionHandlerBase) serviceProvider.GetService(actionHandlerType);

                    if (actionHandler != null)
                    {
                        actionHandler.connection = Connection;
                        actionHandler.executeCommand = command;

                        if (!actionHandlerType.CanExecuteAction(context, actionHandler, serviceProvider))
                        {
                            return command.CreateExceptionResponse<ExecuteResponse>(
                                new UnauthorizedException("User is not allowed to execute actions of this handler"));
                        }

                        if (!actionMethod.CanExecuteAction(context, actionHandler, serviceProvider))
                        {
                            return command.CreateExceptionResponse<ExecuteResponse>(
                                new UnauthorizedException("User is not allowed to execute action"));
                        }

                        return await ExecuteAction(actionHandler, command, actionMethod);
                    }

                    return command.CreateExceptionResponse<ExecuteResponse>(new HandlerNotFoundException());
                }

                return command.CreateExceptionResponse<ExecuteResponse>(new ActionNotFoundException());
            }

            return command.CreateExceptionResponse<ExecuteResponse>(new ActionHandlerNotFoundException());
        }

        private async Task<ResponseBase> ExecuteAction(ActionHandlerBase actionHandler, ExecuteCommand command,
            MethodInfo actionMethod)
        {
            logger.LogInformation("Execution of {0}.{1} started", actionMethod.DeclaringType?.FullName,
                actionMethod.Name);

            object result = actionMethod.Invoke(actionHandler, GetParameters(actionMethod, command));

            if (result != null)
            {
                if (ActionHelper.HandleAsyncEnumerable(result, actionHandler.AsyncResult))
                {
                    result = null;
                }
                else
                {
                    result = await ActionHelper.HandleAsyncResult(result);
                }
            }

            logger.LogInformation("Executed {0}.{1}", actionMethod.DeclaringType?.FullName, actionMethod.Name);

            return new ExecuteResponse()
            {
                ReferenceId = command.ReferenceId,
                Result = result
            };
        }

        private object[] GetParameters(MethodInfo actionMethod, ExecuteCommand command)
        {
            return actionMethod.GetParameters().Select(parameter =>
            {
                if (parameter.Position >= command.Parameters.Length)
                {
                    return null;
                }

                if (parameter.ParameterType.IsGenericType &&
                    parameter.ParameterType.GetGenericTypeDefinition() == typeof(IAsyncEnumerable<>))
                {
                    SapphireStreamHelper streamHelper =
                        (SapphireStreamHelper) serviceProvider.GetService(typeof(SapphireStreamHelper));
                    return streamHelper.OpenStreamChannel(Connection, command, parameter.ParameterType);
                }

                JToken parameterValue = command.Parameters[parameter.Position];
                return parameterValue?.ToObject(parameter.ParameterType);
            }).ToArray();
        }
    }
}