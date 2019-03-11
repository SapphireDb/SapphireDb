using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class CloseConnectionCommand : CommandBase
    {
        public Guid ConnectionId { get; set; }

        public bool DeleteRenewToken { get; set; }
    }
}
