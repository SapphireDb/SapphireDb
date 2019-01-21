using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    class SubscribeRolesResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Roles { get; set; }
    }
}
