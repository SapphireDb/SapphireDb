using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class UnsubscribeMessageCommand : CommandBase
    {
        public string Topic { get; set; }
    }
}
