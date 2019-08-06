using System;

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

        public RealtimeDbContext GetContext(string contextName = "Default")
        {
            return (RealtimeDbContext)serviceProvider.GetService(
                contextTypeContainer.DbContextTypes[contextName.ToLowerInvariant()]);
        }

        public RealtimeDbContext GetContext(Type dbContextType)
        {
            return (RealtimeDbContext)serviceProvider.GetService(dbContextType);
        }
    }
}
