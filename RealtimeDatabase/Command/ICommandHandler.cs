using System.Threading.Tasks;
using RealtimeDatabase.Models;

namespace RealtimeDatabase.Command
{
    // ReSharper disable once TypeParameterCanBeVariant
    internal interface ICommandHandler<T>
        where T : CommandBase
    {
        Task<ResponseBase> Handle(HttpInformation context, T command);
    }
}
