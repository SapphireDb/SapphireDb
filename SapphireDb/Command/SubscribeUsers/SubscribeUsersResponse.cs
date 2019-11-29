using System.Collections.Generic;

namespace SapphireDb.Command.SubscribeUsers
{
    public class SubscribeUsersResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Users { get; set; }
    }
}
