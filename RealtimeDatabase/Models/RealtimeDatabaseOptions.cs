using RealtimeDatabase.Websocket.Models;
using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;

namespace RealtimeDatabase.Models
{
    public class RealtimeDatabaseOptions
    {
        public RealtimeDatabaseOptions()
        {
            EnableAuthCommands = true;
            RequireAuthenticationForAttribute = true;
            AuthInfoAllowFunction = (connection) => true;
            AuthAllowFunction = (connection) => connection.HttpContext.User.IsInRole("admin");
        }

        public string Secret { get; set; }

        public bool AlwaysRequireAuthentication { get; set; }

        public bool RequireAuthenticationForAttribute { get; set; }

        internal bool EnableBuiltinAuth { get; set; }

        public bool EnableAuthCommands { get; set; }

        public Func<WebsocketConnection, bool> AuthInfoAllowFunction { get; set; }

        public Func<WebsocketConnection, bool> AuthAllowFunction { get; set; }

        public bool RestFallback { get; set; }
    }
}
