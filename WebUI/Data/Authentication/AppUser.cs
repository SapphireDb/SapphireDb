using Microsoft.AspNetCore.Identity;
using SapphireDb.Attributes;

namespace WebUI.Data.Authentication
{
    public class AppUser : IdentityUser
    {
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
    }
}
