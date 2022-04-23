using System;
using Microsoft.EntityFrameworkCore;

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

        public DbContext GetContext(string contextName, IServiceProvider customServiceProvider = null)
        {
            return (DbContext)(customServiceProvider ?? serviceProvider).GetService(contextTypeContainer.GetContext(contextName));
        }

        public DbContext GetContext(Type dbContextType, IServiceProvider customServiceProvider = null)
        {
            return (DbContext)(customServiceProvider ?? serviceProvider).GetService(dbContextType);
        }
    }
}
