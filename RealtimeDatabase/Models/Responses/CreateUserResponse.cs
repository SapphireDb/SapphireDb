using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    public class CreateUserResponse : ResponseBase
    {
        public Dictionary<string, object> NewUser { get; set; }

        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
