using System;
using RealtimeDatabase.Models.Responses;
using RealtimeDatabase.Websocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RealtimeDatabase
{
    public class RealtimeDatabaseNotifier
    {
        private readonly WebsocketChangeNotifier notifier;

        public RealtimeDatabaseNotifier(WebsocketChangeNotifier notifier)
        {
            this.notifier = notifier;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            notifier.HandleChanges(changes, dbContextType);
        }
    }
}
