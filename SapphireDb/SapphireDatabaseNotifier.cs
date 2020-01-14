using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Sync;

namespace SapphireDb
{
    public class SapphireDatabaseNotifier
    {
        private readonly SapphireChangeNotifier notifier;
        private readonly SyncManager syncManager;

        public SapphireDatabaseNotifier(SapphireChangeNotifier notifier, SyncManager syncManager)
        {
            this.notifier = notifier;
            this.syncManager = syncManager;
        }

        public void HandleChanges(List<ChangeResponse> changes, Type dbContextType)
        {
            notifier.HandleChanges(changes, dbContextType);
            syncManager.SendChanges(changes, dbContextType);

        }
    }
}
