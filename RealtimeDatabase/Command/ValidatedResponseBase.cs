using System.Collections.Generic;

namespace RealtimeDatabase.Command
{
    public class ValidatedResponseBase : ResponseBase
    {
        public Dictionary<string, List<string>> ValidationResults { get; set; }
    }
}
