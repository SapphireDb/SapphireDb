using System;
using Microsoft.Extensions.DependencyInjection;

namespace SapphireDb.Internal
{
    public class DbContextAccesor
    {
        private readonly DbContextTypeContainer contextTypeContainer;
        private readonly IServiceProvider serviceProvider;

        public DbContextAccesor(DbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.serviceProvider = serviceProvider;
        }

        public SapphireDbContext GetContext(string contextName, IServiceProvider customServiceProvider = null)
        {
            return (SapphireDbContext)(customServiceProvider ?? serviceProvider).GetService(contextTypeContainer.GetContext(contextName));
        }

        public SapphireDbContext GetContext(Type dbContextType, IServiceProvider customServiceProvider = null)
        {
            return (SapphireDbContext)(customServiceProvider ?? serviceProvider).GetService(dbContextType);
        }
    }
}
