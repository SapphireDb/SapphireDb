using RealtimeDatabase.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Auth
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
