using SapphireDb.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SapphireDb.Models;

namespace WebUI.Data.Models
{
    //[Updatable]
    [DeleteEvent(after: "AfterDelete")]
    [CreateEvent(after: "AfterCreate")]
    [UpdateEvent(after: "AfterUpdate")]
    public class User : Base
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [MinLength(3)]
        [QueryAuth(functionName: "Test2")]
        [UpdateAuth(functionName: "Test")]
        [Updateable]
        public string FirstName { get; set; }

        public bool Test(HttpInformation context)
        {
            return context.User.IsInRole("admin");
        }

        public bool Test2(HttpInformation context, RealtimeContext db)
        {
            return db.Users.Count() > 3;
            //return DateTime.UtcNow.Millisecond % 2 == 0;
        }

        [Required]
        [MinLength(3)]
        [Updateable]
        public string LastName { get; set; }

        public void afterCreate(HttpInformation context, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Created user " + Id,
                UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }

        private void AfterUpdate(HttpInformation context, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Updated user " + Id,
                UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }

        public void AfterDelete(HttpInformation context, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Deleted user " + Id,
                UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }
    }
}
