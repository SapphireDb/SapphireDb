using System;
using SapphireDb.Internal;

namespace SapphireDb.Command
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

        public ISapphireAuthContext GetContext()
        {
            return authDbContextAccessor.GetContext();
        }
    }
}
