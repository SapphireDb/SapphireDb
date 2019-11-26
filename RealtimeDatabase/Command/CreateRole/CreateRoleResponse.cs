using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RealtimeDatabase.Command.CreateRole
{
    public class CreateRoleResponse : ResponseBase
    {
        public Dictionary<string, object> NewRole { get; set; }

        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
