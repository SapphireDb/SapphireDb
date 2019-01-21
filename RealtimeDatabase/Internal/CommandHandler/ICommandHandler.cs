using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    interface ICommandHandler<T> where T : CommandBase
    {
        Task Handle(WebsocketConnection websocketConnection, T command);
    }
}
