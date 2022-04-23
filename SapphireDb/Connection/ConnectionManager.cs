using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SapphireDb.Connection
{
    public class ConnectionManager
    {
        private readonly SubscriptionManager subscriptionManager;
        private readonly MessageSubscriptionManager messageSubscriptionManager;
        private readonly ILogger<ConnectionManager> logger;

        private readonly ReaderWriterLock connectionsLock = new ReaderWriterLock();
        public readonly Dictionary<string, SignalRConnection> connections = new Dictionary<string, SignalRConnection>();

        public ConnectionManager(SubscriptionManager subscriptionManager,
            MessageSubscriptionManager messageSubscriptionManager,
            ILogger<ConnectionManager> logger)
        {
            this.subscriptionManager = subscriptionManager;
            this.messageSubscriptionManager = messageSubscriptionManager;
            this.logger = logger;
        }

        public void AddConnection(SignalRConnection connection)
        {
            try
            {
                connectionsLock.AcquireWriterLock(TimeSpan.FromSeconds(1));
                connections.TryAdd(connection.Id, connection);
                
                logger.LogInformation("Added new {ConnectionType} with ConnectionId {SapphireConnectionId}", connection, connection.Id);
                logger.LogDebug("Connection count: {ConnectionCount}", connections.Count);
            }
            finally
            {
                connectionsLock.ReleaseWriterLock();
            }
        }

        public void RemoveConnection(SignalRConnection connection)
        {
            try
            {
                connectionsLock.AcquireWriterLock(TimeSpan.FromSeconds(1));
                string connectionId = connection.Id;
                connections.Remove(connectionId);
                subscriptionManager.RemoveConnectionSubscriptions(connectionId);
                messageSubscriptionManager.RemoveConnectionSubscriptions(connectionId);

                logger.LogInformation("Removed {ConnectionType} with ConnectionId {SapphireConnectionId}", connection, connection.Id);
                logger.LogDebug("Connection count: {ConnectionCount}", connections.Count);
            }
            finally
            {
                connectionsLock.ReleaseWriterLock();
            }

        }

        // public void CheckExistingConnections()
        // {
        //     Task.Run(() =>
        //     {
        //         List<PollConnection> pollConnections;
        //
        //         try
        //         {
        //             connectionsLock.AcquireReaderLock(TimeSpan.FromSeconds(1));
        //
        //             pollConnections = connections.Values
        //                 .Where(connection => connection is PollConnection)
        //                 .Cast<PollConnection>()
        //                 .ToList();
        //         }
        //         finally
        //         {
        //             connectionsLock.ReleaseReaderLock();
        //         }
        //
        //         Parallel.ForEach(pollConnections, pollConnection =>
        //         {
        //             if (pollConnection.ShouldRemove())
        //             {
        //                 RemoveConnection(pollConnection);
        //             }
        //         });
        //     });
        // }

        // public ConnectionBase GetConnection(HttpContext context)
        // {
        //     ConnectionBase connection = null;
        //
        //     string connectionId = context.Request.Headers["connectionId"];
        //     
        //     if (!string.IsNullOrEmpty(connectionId))
        //     {
        //         bool connectionFound;
        //         
        //         try
        //         {
        //             connectionsLock.AcquireReaderLock(TimeSpan.FromSeconds(1));
        //             connectionFound = connections.TryGetValue(connectionId, out connection);
        //         }
        //         finally
        //         {
        //             connectionsLock.ReleaseReaderLock();
        //         }
        //         
        //         if (connectionFound)
        //         {
        //             // Compare user Information of the request and the found connection
        //             if (connection.Information.User.Identity.IsAuthenticated)
        //             {
        //                 List<Claim> connectionClaims = connection.Information.User.Claims.ToList();
        //                 List<Claim> requestClaims = context.User.Claims.ToList();
        //
        //                 if (connectionClaims.Any(connectionClaim =>
        //                 {
        //                     return !requestClaims.Any(claim =>
        //                         claim.Type == connectionClaim.Type && claim.Value == connectionClaim.Value);
        //                 }))
        //                 {
        //                     return null;
        //                 }
        //             }
        //             else
        //             {
        //                 // Compare connection info of request and found connection
        //                 IConnectionInformation connectionInfo = connection.Information;
        //
        //                 if (!connectionInfo.LocalIpAddress.Equals(context.Connection.LocalIpAddress) ||
        //                     !connectionInfo.LocalPort.Equals(context.Connection.LocalPort) ||
        //                     !connectionInfo.RemoteIpAddress.Equals(context.Connection.RemoteIpAddress))
        //                 {
        //                     return null;
        //                 }   
        //             }
        //         }
        //     }
        //
        //     return connection;
        // }
    }
}