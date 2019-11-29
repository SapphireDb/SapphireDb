using SapphireDb.Internal;
using System;
using SapphireDb.Helper;

namespace SapphireDb.Models.Auth
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
