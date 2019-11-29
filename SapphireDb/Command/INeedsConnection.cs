using SapphireDb.Connection;

namespace SapphireDb.Command
{
    interface INeedsConnection
    {
        ConnectionBase Connection { get; set; }
    }
}
