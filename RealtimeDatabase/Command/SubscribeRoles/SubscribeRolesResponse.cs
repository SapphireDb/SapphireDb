using System.Collections.Generic;

namespace RealtimeDatabase.Command.SubscribeRoles
{
    public class SubscribeRolesResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Roles { get; set; }
    }
}
