using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class SubscribeMessageCommand : CommandBase
    {
        public string Topic { get; set; }
    }
}
