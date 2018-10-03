using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    public class ValidatedResponseBase : ResponseBase
    {
        public Dictionary<string, List<string>> ValidationResults { get; set; }
    }
}
