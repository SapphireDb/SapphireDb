using System.Collections.Generic;
using SapphireDb.Command.Subscribe;

namespace SapphireDb.Sync.Models
{
    class SendChangesRequest : SyncRequest
    {
        public string DbName { get; set; }
        public List<ChangeResponse> Changes { get; set; }
    }
}
