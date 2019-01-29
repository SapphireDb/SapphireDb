using RealtimeDatabase.Websocket.Models;
using System;
using Microsoft.AspNetCore.Http;
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
            AuthInfoAllowFunction = (context) => true;
            AuthAllowFunction = (context) => context.User.IsInRole("admin");
        }

        public string Secret { get; set; }

        public bool AlwaysRequireAuthentication { get; set; }

        public bool RequireAuthenticationForAttribute { get; set; }

        internal bool EnableBuiltinAuth { get; set; }

        public bool EnableAuthCommands { get; set; }

        public Func<HttpContext, bool> AuthInfoAllowFunction { get; set; }

        public Func<HttpContext, bool> AuthAllowFunction { get; set; }

        public bool RestFallback { get; set; }
    }
}
