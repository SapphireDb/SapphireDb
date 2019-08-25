using System;
using System.Collections.Concurrent;
using System.Linq;
using RealtimeDatabase.Connection.Websocket;

namespace RealtimeDatabase.Connection
{
    public class RealtimeConnectionManager
    {
        public ConcurrentBag<ConnectionBase> connections;

        public RealtimeConnectionManager()
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
    }
}
