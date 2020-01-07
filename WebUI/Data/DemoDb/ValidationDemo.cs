using System;
using System.ComponentModel.DataAnnotations;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    public class ValidationDemo
    {
        [Key]
        public Guid Id { get; set; }
        
        [Updatable]
        [Required(ErrorMessage = "username is required")]
        [MinLength(3, ErrorMessage = "username min length not reached")]
        public string Username { get; set; }

        [Updatable]
        [EmailAddress(ErrorMessage = "not a valid email address")]
        public string Email { get; set; }

        [Updatable]
        [Required(ErrorMessage = "password is required")]
        [MinLength(5, ErrorMessage = "password min length not reached")]
        public string Password { get; set; }
    }
}