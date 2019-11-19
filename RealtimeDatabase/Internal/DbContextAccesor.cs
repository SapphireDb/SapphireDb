using System;
using Microsoft.Extensions.DependencyInjection;

namespace RealtimeDatabase.Internal
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

        public RealtimeDbContext GetContext(string contextName, IServiceProvider customServiceProvider = null)
        {
            return (RealtimeDbContext)(customServiceProvider ?? serviceProvider).GetService(contextTypeContainer.GetContext(contextName));
        }

        public RealtimeDbContext GetContext(Type dbContextType, IServiceProvider customServiceProvider = null)
        {
            return (RealtimeDbContext)(customServiceProvider ?? serviceProvider).GetService(dbContextType);
        }
    }
}
