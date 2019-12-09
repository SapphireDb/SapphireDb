using System.Collections.Generic;
using SapphireDb.Command.Create;

namespace SapphireDb.Command.CreateRange
{
    public class CreateRangeResponse : ResponseBase
    {
        public List<ResponseBase> Results { get; set; }
    }
}
