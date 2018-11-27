using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class PublishCommand : CommandBase
    {
        public string Topic { get; set; }

        public object Data { get; set; }
    }
}
