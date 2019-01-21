using RealtimeDatabase.Websocket.Models;
using System;

namespace RealtimeDatabase.Models
{
    public class RealtimeDatabaseOptions
    {
        public RealtimeDatabaseOptions()
        {
            AuthInfoAllowFunction = (connection) =>
            {
                return true;
            };

            AuthAllowFunction = (connection) =>
            {
                return connection.HttpContext.User.IsInRole("admin");
            };
        }

        public string Secret { get; set; }

        public AuthenticationMode Authentication { get; set; }

        public bool EnableAuthCommands { get; set; }

        public Func<WebsocketConnection, bool> AuthInfoAllowFunction { get; set; }

        public Func<WebsocketConnection, bool> AuthAllowFunction { get; set; }

        public enum AuthenticationMode
        {
            Custom, AlwaysExceptLogin, Always
        }
    }
}
