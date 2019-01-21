using System;

namespace RealtimeDatabase.Internal
{
    public class DbContextAccesor
    {
        private readonly DbContextTypeContainer contextTypeContainer;
        private readonly IServiceProvider serviceProvider;

        public DbContextAccesor(DbContextTypeContainer _contextTypeContainer, IServiceProvider _serviceProvider)
        {
            contextTypeContainer = _contextTypeContainer;
            serviceProvider = _serviceProvider;
        }

        public RealtimeDbContext GetContext()
        {
            return (RealtimeDbContext)serviceProvider.GetService(contextTypeContainer.DbContextType);
        }
    }
}
