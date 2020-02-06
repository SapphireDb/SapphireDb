using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SapphireDb.Connection.Poll;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class ConnectionManager
    {
        public ConcurrentDictionary<Guid, ConnectionBase> connections;

        public ConnectionManager()
        {
            connections = new ConcurrentDictionary<Guid, ConnectionBase>();
        }

        public void AddConnection(ConnectionBase connection)
        {
            connections.TryAdd(connection.Id, connection);
        }

        public void RemoveConnection(ConnectionBase connection)
        {
            Guid connectionId = connection.Id;
            connections.TryRemove(connectionId, out _);
            connection.Dispose();
        }

        public void CheckExistingConnections()
        {
            foreach (KeyValuePair<Guid, ConnectionBase> connectionValue in connections.Where(c => c.Value is PollConnection))
            {
                PollConnection pollConnection = (PollConnection)connectionValue.Value;

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

                if (connections.TryGetValue(connectionId, out connection))
                {
                    // Compare user Information of the request and the found connection
                    if (connection.Information.User.Identity.IsAuthenticated)
                    {
                        List<Claim> connectionClaims = connection.Information.User.Claims.ToList();
                        List<Claim> requestClaims = context.User.Claims.ToList();

                        if (connectionClaims.Any(connectionClaim =>
                        {
                            return !requestClaims.Any(claim =>
                                claim.Type == connectionClaim.Type && claim.Value == connectionClaim.Value);
                        }))
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
