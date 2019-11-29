using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace SapphireDb.Command.DeleteRole
{
    public class DeleteRoleResponse : ResponseBase
    {
        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
