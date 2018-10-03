using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class CreateResponse : ValidatedResponseBase
    {
        public object NewObject { get; set; }
    }
}
