using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebUI.Helper;

namespace WebUI.Data.Authentication
{
    public class RefreshToken
    {
        public RefreshToken()
        {
            Id = Guid.NewGuid();
            RefreshKey = Guid.NewGuid().ToString().ComputeHash();
            CreatedOn = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public string UserId { get; set; }

        public string RefreshKey { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
