using Microsoft.CSharp.RuntimeBinder;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using RealtimeDatabase.Models.Actions;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Helper;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class ExecuteCommandHandler : CommandHandlerBase, ICommandHandler<ExecuteCommand>, INeedsConnection, IRestFallback
    {
        private readonly ActionMapper actionMapper;
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<ExecuteCommandHandler> logger;
        public ConnectionBase Connection { get; set; }

        public ExecuteCommandHandler(DbContextAccesor contextAccessor, ActionMapper actionMapper, IServiceProvider serviceProvider, ILogger<ExecuteCommandHandler> logger)
            : base(contextAccessor)
        {
            this.actionMapper = actionMapper;
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task<ResponseBase> Handle(HttpContext context, ExecuteCommand command)
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

        private async Task<ResponseBase> GetActionDetails(ExecuteCommand command, HttpContext context)
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
                        actionHandler.connection = Connection;
                        actionHandler.executeCommand = command;

                        if (!actionHandlerType.CanExecuteAction(context, actionHandler, serviceProvider))
                        {
                            return command.CreateExceptionResponse<ExecuteResponse>(
                                "User is not allowed to execute actions of this handler.");
                        }

                        if (!actionMethod.CanExecuteAction(context, actionHandler, serviceProvider))
                        {
                            return command.CreateExceptionResponse<ExecuteResponse>("User is not allowed to execute action.");
                        }

                        return await ExecuteAction(actionHandler, command, actionMethod);
                    }

                    return command.CreateExceptionResponse<ExecuteResponse>("No handler was found.");
                }

                return command.CreateExceptionResponse<ExecuteResponse>("No action to execute was found.");
            }

            return command.CreateExceptionResponse<ExecuteResponse>("No action handler type was matching");
        }

        private async Task<ResponseBase> ExecuteAction(ActionHandlerBase actionHandler, ExecuteCommand command,
            MethodInfo actionMethod)
        {
            logger.LogInformation("Execution of " + actionMethod.DeclaringType.FullName + "." + actionMethod.Name + " started");

            object result = actionMethod.Invoke(actionHandler, GetParameters(actionMethod, command));

            if (result != null)
            {
                try { result = await (dynamic)result; }
                catch { /* Do nothing because result is not an awaitable and already contains the expected result */ }
            }

            logger.LogInformation("Executed " + actionMethod.DeclaringType.FullName + "." + actionMethod.Name);

            return new ExecuteResponse()
            {
                ReferenceId = command.ReferenceId,
                Result = result
            };
        }

        private object[] GetParameters(MethodInfo actionMethod, ExecuteCommand command)
        {
            return actionMethod.GetParameters().Select(parameter => {
                if (parameter.Position >= command.Parameters.Length)
                {
                    return null;
                }

                object parameterValue = command.Parameters[parameter.Position];

                if (parameterValue == null)
                {
                    return null;
                }

                if (parameterValue.GetType() == typeof(JObject))
                {
                    return ((JObject)parameterValue).ToObject(parameter.ParameterType);
                }

                if (parameterValue.GetType() == typeof(JArray))
                {
                    return ((JArray) parameterValue).ToObject(parameter.ParameterType);
                }

                return parameterValue;
            }).ToArray();
        }
    }
}
