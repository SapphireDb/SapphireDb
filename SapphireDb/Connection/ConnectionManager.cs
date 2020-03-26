using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SapphireDb.Connection.Poll;
using SapphireDb.Models;

namespace SapphireDb.Connection
{
    public class ConnectionManager
    {
        private readonly SubscriptionManager subscriptionManager;
        private readonly MessageSubscriptionManager messageSubscriptionManager;
        private readonly ILogger<ConnectionManager> logger;

        private readonly ReaderWriterLockSlim connectionsLock = new ReaderWriterLockSlim();
        public readonly Dictionary<Guid, ConnectionBase> connections = new Dictionary<Guid, ConnectionBase>();

        public ConnectionManager(SubscriptionManager subscriptionManager,
            MessageSubscriptionManager messageSubscriptionManager,
            ILogger<ConnectionManager> logger)
        {
            this.subscriptionManager = subscriptionManager;
            this.messageSubscriptionManager = messageSubscriptionManager;
            this.logger = logger;
        }

        public void AddConnection(ConnectionBase connection)
        {
            CheckExistingConnections();
            
            try
            {
                connectionsLock.EnterWriteLock();
                connections.TryAdd(connection.Id, connection);
                
                logger.LogInformation("Added new {0}", connection);
                logger.LogDebug("Added new {0} with ConnectionId '{1}'", connection, connection.Id);
                logger.LogDebug("Connection count: {0}", connections.Count);
            }
            finally
            {
                connectionsLock.ExitWriteLock();
            }
        }

        public void RemoveConnection(ConnectionBase connection)
        {
            try
            {
                connectionsLock.EnterWriteLock();
                Guid connectionId = connection.Id;
                connections.Remove(connectionId);
                subscriptionManager.RemoveConnectionSubscriptions(connectionId);
                messageSubscriptionManager.RemoveConnectionSubscriptions(connectionId);
                connection.Dispose();
                
                logger.LogInformation("Removed {0}", connection);
                logger.LogDebug("Removed {0} with ConnectionId '{1}'", connection, connection.Id);
                logger.LogDebug("Connection count: {0}", connections.Count);
            }
            finally
            {
                connectionsLock.ExitWriteLock();
            }

        }

        public void CheckExistingConnections()
        {
            Task.Run(() =>
            {
                List<ConnectionBase> connectionsCopy;

                try
                {
                    connectionsLock.EnterReadLock();
                    connectionsCopy = connections.Values.ToList();
                }
                finally
                {
                    connectionsLock.ExitReadLock();
                }

                Parallel.ForEach(connectionsCopy, connection =>
                {
                    if (connection is PollConnection pollConnection)
                    {
                        if (pollConnection.ShouldRemove())
                        {
                            RemoveConnection(pollConnection);
                        }
                    }
                });
            });
        }

        public ConnectionBase GetConnection(HttpContext context)
        {
            ConnectionBase connection = null;

            string connectionIdHeaderValue = context.Request.Headers["connectionId"];
            
            if (!string.IsNullOrEmpty(connectionIdHeaderValue))
            {
                Guid connectionId = Guid.Parse(connectionIdHeaderValue);

                bool connectionFound;
                
                try
                {
                    connectionsLock.EnterReadLock();
                    connectionFound = connections.TryGetValue(connectionId, out connection);
                }
                finally
                {
                    connectionsLock.ExitReadLock();
                }
                
                if (connectionFound)
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