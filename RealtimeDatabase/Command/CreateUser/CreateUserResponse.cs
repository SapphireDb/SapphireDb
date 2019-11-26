using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RealtimeDatabase.Command.CreateUser
{
    public class CreateUserResponse : ResponseBase
    {
        public Dictionary<string, object> NewUser { get; set; }

        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
