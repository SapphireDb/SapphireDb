using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class LoginResponse : ResponseBase
    {
        public LoginResponse()
        {
            UserData = new Dictionary<string, object>();
        }

        public string AuthToken { get; set; }

        public string RefreshToken { get; set; }

        public double ValidFor { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Dictionary<string, object> UserData { get; set; }
    }
}
