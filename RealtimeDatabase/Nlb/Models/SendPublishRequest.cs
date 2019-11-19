using System;
using System.Collections.Generic;
using System.Text;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Nlb.Models
{
    class SendPublishRequest
    {
        public string Topic { get; set; }

        public object Message { get; set; }
    }
}
