using System.Collections.Generic;
using SapphireDb.Command.Delete;

namespace SapphireDb.Command.DeleteRange
{
    public class DeleteRangeResponse : ResponseBase
    {
        public List<DeleteResponse> Results { get; set; }
    }
}
