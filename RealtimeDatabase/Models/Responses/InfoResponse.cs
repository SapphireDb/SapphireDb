using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class InfoResponse : ResponseBase
    {
        public InfoResponse()
        {

        }

        public string[] PrimaryKeys { get; set; }

        public bool OnlyAuthorized { get; set; }

        public string[] RolesRead { get; set; }

        public string[] RolesWrite { get; set; }

        public string[] RolesDelete { get; set; }
    }
}
