using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealtimeDatabase.Attributes;

namespace DemoServerApplication.Data.Models
{
    [Updatable]
    public class User : Base
    {
        public string Username { get; set; }

        public string Firstname { get; set; }

        public string Lastname { get; set; }

        public string Email { get; set; }

    }
}
