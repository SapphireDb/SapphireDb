using System;
using System.Collections.Generic;
using System.Text;
using RealtimeDatabase.Command.Subscribe;

namespace RealtimeDatabase.Nlb.Models
{
    class SendChangesRequest
    {
        public string DbType { get; set; }
        public List<ChangeResponse> Changes { get; set; }
    }
}
