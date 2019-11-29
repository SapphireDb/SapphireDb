using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace SapphireDb.Models
{
    [DataContract]
    public class HttpInformation
    {
        public HttpInformation(HttpContext context)
        {
            User = context.User;
            ClientCertificate = context.Connection.ClientCertificate;
            RemoteIpAddress = context.Connection.RemoteIpAddress;
            LocalIpAddress = context.Connection.LocalIpAddress;
            RemotePort = context.Connection.RemotePort;
            LocalPort = context.Connection.LocalPort;

            if (context.Request.Headers.TryGetValue("User-Agent", out StringValues userAgent))
            {
                UserAgent = userAgent.ToString();
            }

            if (context.User.Identity.IsAuthenticated)
            {
                UserId = context.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value;
            }

            if (context.Request.Query.TryGetValue("key", out StringValues apiKey) || context.Request.Headers.TryGetValue("key", out apiKey))
            {
                SapphireDatabaseOptions options = context.RequestServices.GetService<SapphireDatabaseOptions>();
                ApiName = options.ApiConfigurations.FirstOrDefault(c => c.Key == apiKey.ToString())?.Name;
            }
        }

        public ClaimsPrincipal User { get; set; }

        public  X509Certificate2 ClientCertificate { get; set; }

        [DataMember]
        public int RemotePort { get; set; }

        public IPAddress RemoteIpAddress { get; set; }

        [DataMember]
        public int LocalPort { get; set; }

        public IPAddress LocalIpAddress { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string UserAgent { get; set; }

        [DataMember]
        public string ApiName { get; set; }
    }
}
