using System.Collections.Generic;

namespace RealtimeDatabase.Command.SubscribeUsers
{
    public class SubscribeUsersResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Users { get; set; }
    }
}
