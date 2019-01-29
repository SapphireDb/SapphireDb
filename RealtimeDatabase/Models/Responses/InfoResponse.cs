using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    public class InfoResponse : ResponseBase
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

    public class AuthInfo
    {
        public bool Authentication { get; set; }

        public bool Authorization => Roles != null;

        public string[] Roles { get; set; }

        public string FunctionName { get; set; }
    }

    public class PropertyAuthInfo : AuthInfo
    {
        public Dictionary<string, AuthInfo> Properties { get; set; }
    }
}
