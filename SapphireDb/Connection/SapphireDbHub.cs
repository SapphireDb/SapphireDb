using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SapphireDb.Command;
using SapphireDb.Helper;
using SapphireDb.Internal;

namespace SapphireDb.Connection
{
    class SapphireDbHub : Hub
    {
        private readonly ConnectionManager _connectionManager;
        private readonly CommandExecutor _commandExecutor;
        private readonly ILogger<SapphireDbHub> _logger;

        public SapphireDbHub(ConnectionManager connectionManager,
            CommandExecutor commandExecutor, ILogger<SapphireDbHub> logger)
        {
            _connectionManager = connectionManager;
            _commandExecutor = commandExecutor;
            _logger = logger;
        }
        
        public void ClientMessage(JObject commandJson)
        {
            CommandBase command = JsonHelper.DeserializeCommand(commandJson);

            if (command == null)
            {
                return;
            }

            SignalRConnection connection = GetSignalRConnection();
            AsyncServiceScope? serviceScope = Context.GetHttpContext()?.RequestServices.CreateAsyncScope();

            _ = Task.Run(async () =>
            {
                await using (serviceScope)
                {   
                    ResponseBase response = await _commandExecutor.ExecuteCommand(command, serviceScope?.ServiceProvider, connection, _logger, connection);
            
                    if (response != null)
                    {
                        await connection.Send(response, serviceScope?.ServiceProvider);
                    }
                }
            });
        }

        public override Task OnConnectedAsync()
        {
            SignalRConnection signalRConnection = new SignalRConnection(Context);
            _connectionManager.AddConnection(signalRConnection);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            _connectionManager.RemoveConnection(GetSignalRConnection());
            return base.OnDisconnectedAsync(exception);
        }
        
        private SignalRConnection GetSignalRConnection()
        {
            if (_connectionManager.connections.TryGetValue(Context.ConnectionId,
                    out SignalRConnection signalRConnection))
            {
                return signalRConnection;
            }

            return null;
        }
    }
}