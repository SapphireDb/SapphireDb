using System;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class AuthCommandHandlerBase
    {
        private readonly AuthDbContextAccesor authDbContextAccesor;
        public readonly IServiceProvider serviceProvider;

        public AuthCommandHandlerBase(AuthDbContextAccesor _authDbContextAccesor, IServiceProvider _serviceProvider)
        {
            authDbContextAccesor = _authDbContextAccesor;
            serviceProvider = _serviceProvider;
        }

        public IRealtimeAuthContext GetContext()
        {
            return authDbContextAccesor.GetContext();
        }
    }
}
