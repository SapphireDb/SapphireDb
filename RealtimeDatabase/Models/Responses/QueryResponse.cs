using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class QueryResponse : ResponseBase
    {
        public IEnumerable<object> Collection { get; set; }
    }
}
