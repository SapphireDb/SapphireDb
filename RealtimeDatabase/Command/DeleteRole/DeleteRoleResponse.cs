using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace RealtimeDatabase.Command.DeleteRole
{
    public class DeleteRoleResponse : ResponseBase
    {
        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
