using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class SubscribeUsersResponse : ResponseBase
    {
        public List<Dictionary<string, object>> Users { get; set; }
    }
}
