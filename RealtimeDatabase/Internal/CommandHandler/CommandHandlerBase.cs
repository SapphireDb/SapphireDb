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
        protected readonly WebsocketConnection websocketConnection;

        public CommandHandlerBase(DbContextAccesor _contextAccesor, WebsocketConnection _websocketConnection)
        {
            contextAccesor = _contextAccesor;
            websocketConnection = _websocketConnection;
        }

        protected RealtimeDbContext GetContext()
        {
            return contextAccesor.GetContext();
        }
    }
}
