using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    class SubscribeUsersResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Users { get; set; }
    }
}
