using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NiL.JS.BaseLibrary;
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;

namespace RealtimeDatabase.Internal
{
    class RestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly RealtimeDatabaseOptions options;
        private readonly ILogger<RestMiddleware> logger;
        private readonly Dictionary<string, Type> commandHandlerTypes;
        private readonly Dictionary<string, Type> authCommandHandlerTypes;

        // ReSharper disable once UnusedParameter.Local
        public RestMiddleware(RequestDelegate next, RealtimeDatabaseOptions options, ILogger<RestMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;

            commandHandlerTypes = GetHandlerTypes(typeof(CommandHandlerBase));
            authCommandHandlerTypes = GetHandlerTypes(typeof(AuthCommandHandlerBase));
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
        {
            string requestPath = context.Request.Path.Value.Substring(1).ToLowerInvariant();

            if (context.Request.Method != "POST" || string.IsNullOrEmpty(requestPath))
            {
                await next(context);
                return;
            }

            logger.LogInformation("Started executing " + requestPath);
            ResponseBase response = await LoadHandler(serviceProvider, requestPath, context);

            if (response.Error != null)
            {
                logger.LogError("Error occured while executing " + requestPath, response.Error);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
            }

            logger.LogInformation("Executed " + requestPath);
            await context.Response.WriteAsync(JsonHelper.Serialize(response));
        }

        private bool CanExecuteAuthCommand(Type handlerType, HttpContext context)
        {
            if (options.EnableAuthCommands && handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
            {
                if (!context.User.Identity.IsAuthenticated || !options.AuthAllowFunction(context))
                {
                    return false;
                }
            }

            return true;
        }

        private Dictionary<string, Type> GetHandlerTypes(Type type)
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" &&
                            t.Name.EndsWith("Handler") && t.IsSubclassOf(type) && typeof(IRestFallback).IsAssignableFrom(t))
                .ToDictionary(t => t.Name
                    .Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal))
                    .ToLowerInvariant(), t => t);
        }

        private async Task<ResponseBase> LoadHandler(IServiceProvider serviceProvider, string requestPath, HttpContext context)
        {
            Type handlerType = null;

            if (commandHandlerTypes.ContainsKey(requestPath))
            {
                handlerType = commandHandlerTypes[requestPath];
            }
            else if (authCommandHandlerTypes.ContainsKey(requestPath))
            {
                handlerType = authCommandHandlerTypes[requestPath];

                if (!CanExecuteAuthCommand(handlerType, context))
                {
                    return new ResponseBase()
                    {
                        Error = new Exception("Cannot execute auth command")
                    };
                }
            }

            if (handlerType == null ||
                (options.AlwaysRequireAuthentication && handlerType != typeof(LoginCommandHandler) && !context.User.Identity.IsAuthenticated))
            {
                return new ResponseBase()
                {
                    Error = new Exception("Not allowed")
                };
            }

            return await ConvertRequestBody(context, serviceProvider, handlerType);
        }

        private async Task<ResponseBase> ConvertRequestBody(HttpContext context, IServiceProvider serviceProvider, Type handlerType)
        {
            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new ResponseBase()
                {
                    Error = new Exception("Request body is empty")
                };
            }

            return await Execute(requestBody, serviceProvider, handlerType, context);
           
        }

        private async Task<ResponseBase> Execute(string requestBody, IServiceProvider serviceProvider, Type handlerType, HttpContext context)
        {
            try
            {
                CommandBase command = JsonHelper.DeserialzeCommand(requestBody);

                if (command != null)
                {
                    object handler = serviceProvider.GetService(handlerType);

                    if (handler != null)
                    {
                        return await (dynamic)handlerType.GetMethod("Handle")
                            .Invoke(handler, new object[] { context, command });
                    }
                }

                return new ResponseBase()
                {
                    Error = new Exception("Command could not converted or no handler was found")
                };
            }
            catch (Exception ex)
            {
                return new ResponseBase()
                {
                    Error = ex
                };
            }
        }
    }
}
