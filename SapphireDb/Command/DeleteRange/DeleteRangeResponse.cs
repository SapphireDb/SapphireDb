using System.Collections.Generic;

namespace SapphireDb.Command.DeleteRange
{
    public class DeleteRangeResponse : ResponseBase
    {
        public List<DeleteResponse> Results { get; set; }
    }
    
    public class DeleteResponse : ValidatedResponseBase
    {
        public object Value { get; set; }
    }
}
