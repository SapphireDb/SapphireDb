using System.Threading.Tasks;
using SapphireDb.Models;

namespace SapphireDb.Command
{
    // ReSharper disable once TypeParameterCanBeVariant
    internal interface ICommandHandler<T>
        where T : CommandBase
    {
        Task<ResponseBase> Handle(HttpInformation context, T command, ExecutionContext executionContext);
    }
}
