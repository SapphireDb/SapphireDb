using System;
using System.Collections.Generic;
using System.Text;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Nlb.Models
{
    class SendChangesRequest
    {
        public string DbType { get; set; }
        public List<ChangeResponse> Changes { get; set; }
    }
}
