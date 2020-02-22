using System.Collections.Generic;
using SapphireDb.Command.Update;

namespace SapphireDb.Command.UpdateRange
{
    public class UpdateRangeResponse : ResponseBase
    {
        public List<ValidatedResponseBase> Results { get; set; }
    }
}
