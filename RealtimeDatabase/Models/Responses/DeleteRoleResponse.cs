using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace RealtimeDatabase.Models.Responses
{
    class DeleteRoleResponse : ResponseBase
    {
        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
