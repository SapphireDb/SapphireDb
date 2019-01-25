using System;

namespace RealtimeDatabase.Internal.CommandHandler
{
    class AuthCommandHandlerBase
    {
        private readonly AuthDbContextAccesor authDbContextAccessor;
        public readonly IServiceProvider serviceProvider;

        public AuthCommandHandlerBase(AuthDbContextAccesor authDbContextAccessor, IServiceProvider serviceProvider)
        {
            this.authDbContextAccessor = authDbContextAccessor;
            this.serviceProvider = serviceProvider;
        }

        public IRealtimeAuthContext GetContext()
        {
            return authDbContextAccessor.GetContext();
        }
    }
}
