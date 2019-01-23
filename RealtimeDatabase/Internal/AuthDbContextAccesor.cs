using System;

namespace RealtimeDatabase.Internal
{
    class AuthDbContextAccesor
    {
        private readonly AuthDbContextTypeContainer contextTypeContainer;
        private readonly IServiceProvider serviceProvider;

        public AuthDbContextAccesor(AuthDbContextTypeContainer contextTypeContainer, IServiceProvider serviceProvider)
        {
            this.contextTypeContainer = contextTypeContainer;
            this.serviceProvider = serviceProvider;
        }

        public IRealtimeAuthContext GetContext()
        {
            return (IRealtimeAuthContext)serviceProvider.GetService(contextTypeContainer.DbContextType);
        }
    }
}
