using System;
using System.Collections.Generic;
using SapphireDb.Command.Subscribe;
using SapphireDb.Sync.Models;

namespace SapphireDb.Sync
{
    public interface ISapphireSyncModule
    {
        void Publish(SyncRequest syncRequest);

        public delegate void SyncRequestReceivedHandler(SyncRequest syncRequest);
        event SyncRequestReceivedHandler SyncRequestRequestReceived;
    }
}