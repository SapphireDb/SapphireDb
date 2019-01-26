using RealtimeDatabase.Models.Commands;
using RealtimeDatabase.Websocket.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    // ReSharper disable once TypeParameterCanBeVariant
    internal interface ICommandHandler<T>
        where T : CommandBase
    {
        Task<ResponseBase> Handle(HttpContext context, T command);
    }
}
