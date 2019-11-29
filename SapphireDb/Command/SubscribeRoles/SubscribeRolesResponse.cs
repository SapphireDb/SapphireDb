using System.Collections.Generic;

namespace SapphireDb.Command.SubscribeRoles
{
    public class SubscribeRolesResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Roles { get; set; }
    }
}
