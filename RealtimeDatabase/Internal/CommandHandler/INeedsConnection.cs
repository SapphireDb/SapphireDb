using System;
using System.Collections.Generic;
using System.Text;
using RealtimeDatabase.Connection;

namespace RealtimeDatabase.Internal.CommandHandler
{
    interface INeedsConnection
    {
        ConnectionBase Connection { get; set; }
    }
}
