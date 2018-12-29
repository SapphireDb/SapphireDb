using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RealtimeDatabase.Websocket;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
