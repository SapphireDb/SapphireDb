using System;

namespace RealtimeDatabase.Internal
{
    class AuthDbContextAccesor
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly IServiceProvider serviceProvider;

        public AuthDbContextAccesor(AuthDbContextTypeContainer _contextTypeContainer, IServiceProvider _serviceProvider)
        {
            contextTypeContainer = _contextTypeContainer;
            serviceProvider = _serviceProvider;
        }

        public IRealtimeAuthContext GetContext()
        {
            return (IRealtimeAuthContext)serviceProvider.GetService(contextTypeContainer.DbContextType);
        }
    }
}
