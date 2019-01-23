using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    // ReSharper disable once TypeParameterCanBeVariant
    internal interface ICommandHandler<T> where T : CommandBase
    {
        Task Handle(WebsocketConnection websocketConnection, T command);
    }
}
