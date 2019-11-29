using System;

namespace SapphireDb.Internal
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

        public ISapphireAuthContext GetContext()
        {
            return (ISapphireAuthContext)serviceProvider.GetService(contextTypeContainer.DbContextType);
        }
    }
}
