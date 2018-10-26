using Microsoft.AspNetCore.Authorization;
using RealtimeDatabase;
using RealtimeDatabase.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebUI.Data.Models
{
    [Updatable]
    public class User : Base
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [MinLength(3)]
        [QueryAuth("admin")]
        [UpdateAuth("admin")]
        [Updatable]
        public string FirstName { get; set; }

        [Required]
        [MinLength(3)]
        public string LastName { get; set; }
    }
}
