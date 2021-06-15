using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SapphireDb.Attributes;
using SapphireDb.Helper;
using SapphireDb.Internal;
using SapphireDb.Models;
using SapphireDb.Models.Exceptions;

namespace SapphireDb.Command.Invoke
{
    class InvokeCommandHandler : CommandHandlerBase, ICommandHandler<InvokeCommand>
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<InvokeCommandHandler> logger;

        public InvokeCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider,
            ILogger<InvokeCommandHandler> logger) : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.logger = logger;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, InvokeCommand command, ExecutionContext executionContext)
        {
            SapphireDbContext db = GetContext(command.ContextName);
            KeyValuePair<Type, string> property = db.GetType().GetDbSetType(command.CollectionName);

            if (property.Key == null)
            {
                throw new CollectionNotFoundException(command.ContextName, command.CollectionName);
            }
            
            object[] primaryKeys = property.Key.GetPrimaryKeyValues(db, command.PrimaryKeys);
            object value = db.Find(property.Key, primaryKeys);

            if (value == null)
            {
                throw new ValueNotFoundException(command.ContextName, command.CollectionName, primaryKeys);
            }

            MethodInfo methodInfo = ReflectionMethodHelper.GetMethodInfo(property.Key, command.Action);

            if (methodInfo == null)
            {
                throw new MethodNotFoundException(property.Key.Name, command.Action);
            }
            
            if (methodInfo.GetCustomAttribute<InvokableAttribute>() == null)
            {
                throw new NotInvokableException(command.ContextName, command.CollectionName, command.Action);
            }

            object result = methodInfo.Invoke(value, methodInfo.CreateParameters(context, serviceProvider, new object[]{ command.Parameters }));
            
            if (result != null)
            {
                result = await ActionHelper.HandleAsyncResult(result);
            }
            
            logger.LogInformation("Executed {CollectionName}.{ActionName}. ExecutionId: {ExecutionId}", property.Value, methodInfo.Name, executionContext.Id);

            return new InvokeResponse()
            {
                ReferenceId = command.ReferenceId,
                Result = result
            };
        }
    }
}