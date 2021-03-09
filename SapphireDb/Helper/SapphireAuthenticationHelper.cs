using Microsoft.AspNetCore.Http;
using SapphireDb.Connection.Websocket;

namespace SapphireDb.Helper
{
    public static class SapphireAuthenticationHelper
    {
        public static string GetWebsocketAuthorizationHeader(HttpRequest request)
        {
            return WebsocketHelper.GetCustomHeader(request, "authorization");
        }
    }
}