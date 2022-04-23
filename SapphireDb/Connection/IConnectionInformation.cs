using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SapphireDb.Connection
{
    public interface IConnectionInformation
    {
        public ClaimsPrincipal User { get; }

        public HttpContext HttpContext { get; }
    }
}