using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    public class ValidatedResponseBase : ResponseBase
    {
        public Dictionary<string, List<string>> ValidationResults { get; set; }
    }
}
