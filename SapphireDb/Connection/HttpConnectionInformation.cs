using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SapphireDb.Connection
{
    public class HttpConnectionInformation : IConnectionInformation
    {
        private readonly HttpContext _httpContext;

        public HttpConnectionInformation(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public ClaimsPrincipal User => _httpContext.User;
        public HttpContext HttpContext => _httpContext;
    }
}