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
using RealtimeDatabase.Helper;
using RealtimeDatabase.Internal;
using RealtimeDatabase.Internal.CommandHandler;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Connection
{
    class RestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly RealtimeDatabaseOptions options;
        private readonly ILogger<RestMiddleware> logger;
        //private readonly Dictionary<string, Type> commandHandlerTypes;
        //private readonly Dictionary<string, Type> authCommandHandlerTypes;

        // ReSharper disable once UnusedParameter.Local
        public RestMiddleware(RequestDelegate next, RealtimeDatabaseOptions options, ILogger<RestMiddleware> logger)
        {
            this.next = next;
            this.options = options;
            this.logger = logger;

            //commandHandlerTypes = GetHandlerTypes(typeof(CommandHandlerBase));
            //authCommandHandlerTypes = GetHandlerTypes(typeof(AuthCommandHandlerBase));
        }

        public async Task Invoke(HttpContext context, IServiceProvider serviceProvider, CommandHandlerMapper commandHandlerMapper)
        {
            string requestPath = context.Request.Path.Value.Substring(1).ToLowerInvariant();

            if (context.Request.Method != "POST" || string.IsNullOrEmpty(requestPath))
            {
                await next(context);
                return;
            }

            if (!string.IsNullOrEmpty(options.Secret))
            {
                if (context.Request.Headers["secret"] != options.Secret)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("The secret does not match");
                    return;
                }
            }

            StreamReader sr = new StreamReader(context.Request.Body);
            string requestBody = await sr.ReadToEndAsync();

            CommandBase command = JsonHelper.DeserialzeCommand(requestBody);
            if (command != null)
            {
                ResponseBase response = await commandHandlerMapper.ExecuteCommand(command, serviceProvider, context, logger);

                if (response?.Error != null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                }

                await context.Response.WriteAsync(JsonHelper.Serialize(response));
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Parsing of the command was not successful");
            }
        }

        //private bool CanExecuteAuthCommand(Type handlerType, HttpContext context)
        //{
        //    if (options.EnableAuthCommands && handlerType != typeof(LoginCommandHandler) && handlerType != typeof(RenewCommandHandler))
        //    {
        //        if (!context.User.Identity.IsAuthenticated || !options.AuthAllowFunction(context))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        //private Dictionary<string, Type> GetHandlerTypes(Type type)
        //{
        //    return Assembly.GetExecutingAssembly().GetTypes()
        //        .Where(t => t.Namespace == "RealtimeDatabase.Internal.CommandHandler" &&
        //                    t.Name.EndsWith("Handler") && t.IsSubclassOf(type) && typeof(IRestFallback).IsAssignableFrom(t))
        //        .ToDictionary(t => t.Name
        //            .Substring(0, t.Name.LastIndexOf("Handler", StringComparison.Ordinal))
        //            .ToLowerInvariant(), t => t);
        //}

        //private async Task<ResponseBase> LoadHandler(IServiceProvider serviceProvider, string requestPath, HttpContext context)
        //{
        //    Type handlerType = null;

        //    if (commandHandlerTypes.ContainsKey(requestPath))
        //    {
        //        handlerType = commandHandlerTypes[requestPath];
        //    }
        //    else if (authCommandHandlerTypes.ContainsKey(requestPath))
        //    {
        //        handlerType = authCommandHandlerTypes[requestPath];

        //        if (!CanExecuteAuthCommand(handlerType, context))
        //        {
        //            return new ResponseBase()
        //            {
        //                Error = new Exception("Cannot execute auth command")
        //            };
        //        }
        //    }

        //    if (handlerType == null ||
        //        (options.AlwaysRequireAuthentication && handlerType != typeof(LoginCommandHandler) && !context.User.Identity.IsAuthenticated))
        //    {
        //        return new ResponseBase()
        //        {
        //            Error = new Exception("Not allowed")
        //        };
        //    }

        //    return await ConvertRequestBody(context, serviceProvider, handlerType);
        //}

        //private async Task<ResponseBase> ConvertRequestBody(HttpContext context, IServiceProvider serviceProvider, Type handlerType)
        //{
        //    StreamReader sr = new StreamReader(context.Request.Body);
        //    string requestBody = await sr.ReadToEndAsync();
        //    return await Execute(requestBody, serviceProvider, handlerType, context);
        //}

        //private async Task<ResponseBase> Execute(string requestBody, IServiceProvider serviceProvider, Type handlerType, HttpContext context)
        //{
        //    try
        //    {
        //        Type commandType = handlerType.GetInterfaces().FirstOrDefault(i => i.Name.StartsWith("ICommandHandler"))?.GetGenericArguments()?[0];

        //        CommandBase command;
        //        if (string.IsNullOrEmpty(requestBody))
        //        {
        //            command = (CommandBase)Activator.CreateInstance(commandType);
        //        }
        //        else
        //        {
        //            command = (CommandBase)JsonHelper.Deserialze(requestBody, commandType);
        //        }

        //        if (command != null)
        //        {
        //            object handler = serviceProvider.GetService(handlerType);

        //            if (handler != null)
        //            {
        //                return await (dynamic)handlerType.GetMethod("Handle")
        //                    .Invoke(handler, new object[] { context, command });
        //            }
        //        }

        //        return new ResponseBase()
        //        {
        //            Error = new Exception("Command could not converted or no handler was found")
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseBase()
        //        {
        //            Error = ex
        //        };
        //    }
        //}
    }
}
