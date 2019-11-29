using System.Collections.Generic;

namespace SapphireDb.Command
{
    public class ValidatedResponseBase : ResponseBase
    {
        public Dictionary<string, List<string>> ValidationResults { get; set; }
    }
}
