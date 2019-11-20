using RealtimeDatabase.Models.Commands;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RealtimeDatabase.Models;
using RealtimeDatabase.Models.Responses;

namespace RealtimeDatabase.Internal.CommandHandler
{
    // ReSharper disable once TypeParameterCanBeVariant
    internal interface ICommandHandler<T>
        where T : CommandBase
    {
        Task<ResponseBase> Handle(HttpInformation context, T command);
    }
}
