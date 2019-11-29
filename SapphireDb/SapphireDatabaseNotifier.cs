using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SapphireDb.Command.Subscribe;
using SapphireDb.Connection;
using SapphireDb.Nlb;

namespace SapphireDb
{
    public class SapphireDatabaseNotifier
    {
        private readonly SapphireChangeNotifier notifier;
        private readonly NlbManager nlbManager;

        public SapphireDatabaseNotifier(SapphireChangeNotifier notifier, NlbManager nlbManager)
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
