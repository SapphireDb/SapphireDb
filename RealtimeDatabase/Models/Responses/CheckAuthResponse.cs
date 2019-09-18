using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    public class CheckAuthResponse : ResponseBase
    {
        public bool Authenticated { get; set; }
    }
}
