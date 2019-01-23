using System;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class AuthCommandHandlerBase
    {
        private readonly AuthDbContextAccesor authDbContextAccesor;
        public readonly IServiceProvider serviceProvider;

        public AuthCommandHandlerBase(AuthDbContextAccesor authDbContextAccesor, IServiceProvider serviceProvider)
        {
            this.authDbContextAccesor = authDbContextAccesor;
            this.serviceProvider = serviceProvider;
        }

        public IRealtimeAuthContext GetContext()
        {
            return authDbContextAccesor.GetContext();
        }
    }
}
