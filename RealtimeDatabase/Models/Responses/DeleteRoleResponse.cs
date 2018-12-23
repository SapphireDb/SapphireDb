using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class DeleteRoleResponse : ResponseBase
    {
        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
