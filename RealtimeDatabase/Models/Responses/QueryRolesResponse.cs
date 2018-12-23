using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class QueryRolesReponse : ResponseBase
    {
        public List<Dictionary<string, object>> Roles { get; set; }
    }
}
