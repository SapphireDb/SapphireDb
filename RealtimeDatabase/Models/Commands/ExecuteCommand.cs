using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class ExecuteCommand : CommandBase
    {
        public string ActionHandlerName { get; set; }

        public string ActionName { get; set; }

        public object[] Parameters { get; set; }
    }
}
