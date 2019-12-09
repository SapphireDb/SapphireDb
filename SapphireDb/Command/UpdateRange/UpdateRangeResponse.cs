using System.Collections.Generic;

namespace SapphireDb.Command.UpdateRange
{
    public class UpdateRangeResponse : ResponseBase
    {
        public List<ResponseBase> Results { get; set; }
    }
}
