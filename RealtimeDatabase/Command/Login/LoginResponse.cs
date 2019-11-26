using System;
using System.Collections.Generic;

namespace RealtimeDatabase.Command.Login
{
    public class LoginResponse : ResponseBase
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
