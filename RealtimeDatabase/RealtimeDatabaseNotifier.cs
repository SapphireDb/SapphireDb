using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RealtimeDatabase.Command.Subscribe;
using RealtimeDatabase.Connection;
using RealtimeDatabase.Nlb;

namespace RealtimeDatabase
{
    public class RealtimeDatabaseNotifier
    {
        private readonly RealtimeChangeNotifier notifier;
        private readonly NlbManager nlbManager;

        public RealtimeDatabaseNotifier(RealtimeChangeNotifier notifier, NlbManager nlbManager)
        {
            this.notifier = notifier;
            this.nlbManager = nlbManager;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            notifier.HandleChanges(changes, dbContextType);
            nlbManager.SendChanges(changes, dbContextType);

        }
    }
}
