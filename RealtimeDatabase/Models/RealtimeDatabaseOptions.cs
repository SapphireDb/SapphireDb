using RealtimeDatabase.Websocket.Models;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealtimeDatabase.Models.Auth;
using Microsoft.Extensions.Configuration;

namespace RealtimeDatabase.Models
{
    public class RealtimeDatabaseOptions
    {
        public RealtimeDatabaseOptions()
        {
            EnableAuthCommands = true;
            AuthInfoAllowFunction = (context) => true;
            AuthAllowFunction = (context) => context.User.IsInRole("admin");
            IsAllowedToSendMessages = (context) => context.User.Identity.IsAuthenticated;
            IsAllowedForTopicSubscribe = (context, topic) => context.User.Identity.IsAuthenticated;
            IsAllowedForTopicPublish = (context, topic) => context.User.Identity.IsAuthenticated;
        }

        public RealtimeDatabaseOptions(IConfigurationSection configuration) : this()
        {
            Secret = configuration[nameof(Secret)];
            AlwaysRequireAuthentication = configuration.GetValue<bool>(nameof(AlwaysRequireAuthentication));
            EnableAuthCommands = configuration[nameof(EnableAuthCommands)]?.ToLowerInvariant() != "false";
            RestFallback = configuration.GetValue<bool>(nameof(RestFallback));
        }

        public string Secret { get; set; }

        public bool AlwaysRequireAuthentication { get; set; }

        internal bool EnableBuiltinAuth { get; set; }

        public bool EnableAuthCommands { get; set; }

        public Func<HttpContext, bool> AuthInfoAllowFunction { get; set; }

        public Func<HttpContext, bool> AuthAllowFunction { get; set; }

        public Func<HttpContext, bool> IsAllowedToSendMessages { get; set; }

        public Func<HttpContext, string, bool> IsAllowedForTopicSubscribe { get; set; }

        public Func<HttpContext, string, bool> IsAllowedForTopicPublish { get; set; }

        public bool RestFallback { get; set; }
    }
}
