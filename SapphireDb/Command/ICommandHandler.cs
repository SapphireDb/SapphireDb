using System.Threading.Tasks;
using SapphireDb.Connection;
using SapphireDb.Models;

namespace SapphireDb.Command
{
    // ReSharper disable once TypeParameterCanBeVariant
    internal interface ICommandHandler<T>
        where T : CommandBase
    {
        Task<ResponseBase> Handle(IConnectionInformation context, T command, ExecutionContext executionContext);
    }
}
