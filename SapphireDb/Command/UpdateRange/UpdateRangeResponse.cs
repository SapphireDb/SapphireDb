using System.Collections.Generic;

namespace SapphireDb.Command.UpdateRange
{
    public class UpdateRangeResponse : ResponseBase
    {
        public List<ValidatedResponseBase> Results { get; set; }
    }
    
    public class UpdateResponse : ValidatedResponseBase
    {
        public object Value { get; set; }
    }
}
