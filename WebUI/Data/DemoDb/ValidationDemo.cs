using System;
using System.ComponentModel.DataAnnotations;
using SapphireDb.Attributes;
using SapphireDb.Models;

namespace WebUI.Data.DemoDb
{
    // [DisableAutoMerge]
    public class ValidationDemo : SapphireOfflineEntity
    {
        [Updatable]
        [Required(ErrorMessage = "username is required")]
        [MinLength(3, ErrorMessage = "username min length not reached")]
        [MergeConflictResolutionMode(MergeConflictResolutionMode.ConflictMarkers)]
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