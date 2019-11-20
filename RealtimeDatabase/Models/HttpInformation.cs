using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace RealtimeDatabase.Models
{
    [DataContract]
    public class HttpInformation
    {
        public HttpInformation(HttpContext context)
        {
            User = context.User;
            Connection = context.Connection;

            if (context.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent))
            {
                UserAgent = userAgent.ToString();
            }

            if (context.User.Identity.IsAuthenticated)
            {
                UserId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            }

            if (context.Request.Query.TryGetValue("key", out StringValues apiKey))
            {
                RealtimeDatabaseOptions options = context.RequestServices.GetService<RealtimeDatabaseOptions>();
                ApiName = options.ApiConfigurations.FirstOrDefault(c => c.Key == apiKey.ToString())?.Name;
            }
        }

        public ClaimsPrincipal User { get; set; }

        public ConnectionInfo Connection { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string UserAgent { get; set; }

        [DataMember]
        public string ApiName { get; set; }
    }
}
