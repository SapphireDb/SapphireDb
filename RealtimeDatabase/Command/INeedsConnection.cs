using RealtimeDatabase.Connection;

namespace RealtimeDatabase.Command
{
    interface INeedsConnection
    {
        ConnectionBase Connection { get; set; }
    }
}
