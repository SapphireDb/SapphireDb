using System.Collections.Generic;

namespace SapphireDb.Command.DeleteRange
{
    public class DeleteRangeResponse : ResponseBase
    {
        public List<ResponseBase> Results { get; set; }
    }
}
