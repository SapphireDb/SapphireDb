using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class UnloadResponse : ResponseBase
    {
        public object[] PrimaryValues { get; set; }
    }
}
