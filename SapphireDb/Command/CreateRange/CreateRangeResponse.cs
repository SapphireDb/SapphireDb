using System.Collections.Generic;

namespace SapphireDb.Command.CreateRange
{
    public class CreateRangeResponse : ResponseBase
    {
        public List<CreateResponse> Results { get; set; }
    }
    
    public class CreateResponse : ValidatedResponseBase
    {
        
    }
}
