using SapphireDb.Connection;

namespace SapphireDb.Command
{
    interface INeedsConnection
    {
        SignalRConnection Connection { get; set; }
    }
}
