using System;
using RealtimeDatabase.Models.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealtimeDatabase.Connection;

namespace RealtimeDatabase
{
    public class RealtimeDatabaseNotifier
    {
        private readonly RealtimeChangeNotifier notifier;

        public RealtimeDatabaseNotifier(RealtimeChangeNotifier notifier)
        {
            this.notifier = notifier;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            notifier.HandleChanges(changes, dbContextType);
        }
    }
}
