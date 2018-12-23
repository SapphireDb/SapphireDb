using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Responses
{
    class CreateRoleResponse : ResponseBase
    {
        public Dictionary<string, object> NewRole { get; set; }

        public IEnumerable<IdentityError> IdentityErrors { get; set; }
    }
}
