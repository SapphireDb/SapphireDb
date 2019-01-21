using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    class QueryResponse : ResponseBase
    {
        public IEnumerable<object> Collection { get; set; }
    }
}
