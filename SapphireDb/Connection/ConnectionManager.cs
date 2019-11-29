using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using SapphireDb.Connection.Poll;
using SapphireDb.Connection.Websocket;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class ConnectionManager
    {
        public ConcurrentBag<ConnectionBase> connections;

        public ConnectionManager()
        {
            connections = new ConcurrentBag<ConnectionBase>();
        }

        public void AddConnection(ConnectionBase connection)
        {
            connections.Add(connection);
        }

        public void RemoveConnection(ConnectionBase connection)
        {
            Guid connectionId = connection.Id;
            connections = new ConcurrentBag<ConnectionBase>(connections.Where(c => c.Id != connectionId));
            connection.Dispose();
        }

        public void CheckExistingConnections()
        {
            foreach (ConnectionBase connectionBase in connections.Where(c => c is PollConnection))
            {
                PollConnection pollConnection = (PollConnection)connectionBase;

                if (pollConnection.lastPoll < DateTime.UtcNow.AddMinutes(-2d))
                {
                    RemoveConnection(pollConnection);
                }
            }
        }

        public ConnectionBase GetConnection(HttpContext context)
        {
            ConnectionBase connection = null;

            if (!string.IsNullOrEmpty(context.Request.Headers["connectionId"]))
            {
                Guid connectionId = Guid.Parse(context.Request.Headers["connectionId"]);
                connection = connections.FirstOrDefault(c => c.Id == connectionId);

                if (connection != null)
                {
                    // Compare user Information of the request and the found connection
                    if (connection.Information.User.Identity.IsAuthenticated)
                    {
                        ClaimsPrincipal connectionUser = connection.Information.User;

                        if (connectionUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value !=
                            context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value ||
                            connectionUser.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value !=
                            context.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value ||
                            connectionUser.Claims.FirstOrDefault(c => c.Type == "Id")?.Value !=
                            context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value)
                        {
                            return null;
                        }
                    }

                    // Compare connection info of request and found connection
                    HttpInformation connectionInfo = connection.Information;

                    if (!connectionInfo.LocalIpAddress.Equals(context.Connection.LocalIpAddress) ||
                        !connectionInfo.LocalPort.Equals(context.Connection.LocalPort) ||
                        !connectionInfo.RemoteIpAddress.Equals(context.Connection.RemoteIpAddress))
                    {
                        return null;
                    }
                }
            }

            return connection;
        }
    }
}
