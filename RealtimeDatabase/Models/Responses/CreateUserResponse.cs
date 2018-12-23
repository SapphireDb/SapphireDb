using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class CreateUserResponse : ResponseBase
    {
        public Dictionary<string, object> NewUser { get; set; }

        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
