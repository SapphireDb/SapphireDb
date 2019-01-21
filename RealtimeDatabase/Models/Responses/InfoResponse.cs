using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    class InfoResponse : ResponseBase
    {
        public InfoResponse()
        {

        }

        public string[] PrimaryKeys { get; set; }

        public PropertyAuthInfo QueryAuth { get; set; }

        public AuthInfo CreateAuth { get; set; }

        public AuthInfo RemoveAuth { get; set; }

        public PropertyAuthInfo UpdateAuth { get; set; }
    }

    class AuthInfo
    {
        public bool Authentication { get; set; }

        public bool Authorization {
            get {
                return Roles != null;
            }
        }

        public string[] Roles { get; set; }

        public string FunctionName { get; set; }
    }

    class PropertyAuthInfo : AuthInfo
    {
        public Dictionary<string, AuthInfo> Properties { get; set; }
    }
}
