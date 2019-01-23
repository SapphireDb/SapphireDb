using Microsoft.AspNetCore.Identity;
using RealtimeDatabase.Attributes;

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
