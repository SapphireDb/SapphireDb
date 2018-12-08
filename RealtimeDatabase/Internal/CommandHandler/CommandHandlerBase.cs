using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class CommandHandlerBase
    {
        protected readonly DbContextAccesor contextAccesor;

        public CommandHandlerBase(DbContextAccesor _contextAccesor)
        {
            contextAccesor = _contextAccesor;
        }

        protected RealtimeDbContext GetContext()
        {
            return contextAccesor.GetContext();
        }

        public Task SendMessage(WebsocketConnection websocketConnection, object message)
        {
            lock (websocketConnection)
            {
                return websocketConnection.Websocket.Send(message);
            }
        }
    }
}
