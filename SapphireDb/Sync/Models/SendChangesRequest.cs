using System;
using System.Collections.Generic;
using System.Text;
using SapphireDb.Command.Subscribe;

namespace SapphireDb.Sync.Models
{
    class SendChangesRequest
    {
        public string DbType { get; set; }
        public List<ChangeResponse> Changes { get; set; }
    }
}
