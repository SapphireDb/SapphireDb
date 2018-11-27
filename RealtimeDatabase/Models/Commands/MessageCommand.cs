using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class MessageCommand : CommandBase
    {
        public object Data { get; set; }
    }
}
