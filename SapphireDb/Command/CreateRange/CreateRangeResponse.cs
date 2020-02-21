using System.Collections.Generic;
using SapphireDb.Command.Create;

namespace SapphireDb.Command.CreateRange
{
    public class CreateRangeResponse : ResponseBase
    {
        public List<CreateResponse> Results { get; set; }
    }
}
