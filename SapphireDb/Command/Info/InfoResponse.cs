using System.Collections.Generic;

namespace SapphireDb.Command.Info
{
    public class InfoResponse : ResponseBase
    {
        public InfoResponse()
        {

        }

        public string[] PrimaryKeys { get; set; }
    }
}
