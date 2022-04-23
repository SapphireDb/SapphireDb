using Microsoft.EntityFrameworkCore;
using SapphireDb.Internal;

namespace SapphireDb.Command
{
    class CommandHandlerBase
    {
        protected readonly DbContextAccesor contextAccessor;

        public CommandHandlerBase(DbContextAccesor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        protected DbContext GetContext(string contextName)
        {
            return contextAccessor.GetContext(contextName);
        }
    }
}
