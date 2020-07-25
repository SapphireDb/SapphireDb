using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SapphireDb.Connection;
using SapphireDb.Internal;
using SapphireDb.Models;

namespace SapphireDb.Command.ExecuteCommands
{
    class ExecuteCommandsCommandHandler : CommandHandlerBase, ICommandHandler<ExecuteCommandsCommand>, INeedsConnection
    {
        private readonly IServiceProvider serviceProvider;
        private readonly CommandExecutor commandExecutor;
        private readonly ILogger<ExecuteCommandsCommandHandler> logger;
        public ConnectionBase Connection { get; set; }

        public ExecuteCommandsCommandHandler(DbContextAccesor contextAccessor, IServiceProvider serviceProvider, CommandExecutor commandExecutor, ILogger<ExecuteCommandsCommandHandler> logger)
            : base(contextAccessor)
        {
            this.serviceProvider = serviceProvider;
            this.commandExecutor = commandExecutor;
            this.logger = logger;
        }

        public async Task<ResponseBase> Handle(HttpInformation context, ExecuteCommandsCommand command,
            ExecutionContext executionContext)
        {
            List<ExecuteCommandsResultResponse> results = new List<ExecuteCommandsResultResponse>();
            
            foreach (CommandBase cmd in command.Commands)
            {
                ResponseBase response =
                    await commandExecutor.ExecuteCommand(cmd, serviceProvider, context, logger, Connection);
                results.Add(new ExecuteCommandsResultResponse()
                {
                    Command = cmd,
                    Response = response
                });
            }
            
            return new ExecuteCommandsResponse()
            {
                Results = results,
                ReferenceId = command.ReferenceId
            };
        }
    }
}
