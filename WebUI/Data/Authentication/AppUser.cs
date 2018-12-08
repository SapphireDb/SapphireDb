using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Data.Authentication
{
    public class AppUser : IdentityUser
    {
        [AuthUserInformation]
        [AuthClaimInformation]
        public string FirstName { get; set; }

        [AuthUserInformation]
        public string LastName { get; set; }
    }
}
