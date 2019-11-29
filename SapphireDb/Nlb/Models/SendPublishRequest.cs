using System;
using System.Collections.Generic;
using System.Text;

namespace SapphireDb.Nlb.Models
{
    class SendPublishRequest
    {
        public string Topic { get; set; }

        public object Message { get; set; }
    }
}
