using RealtimeDatabase.Models.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RealtimeDatabase.Internal.CommandHandler
{
    interface ICommandHandler<T> where T : CommandBase
    {
        Task Handle(T command);
    }
}
