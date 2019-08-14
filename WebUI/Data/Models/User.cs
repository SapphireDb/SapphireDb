using RealtimeDatabase.Attributes;
using RealtimeDatabase.Websocket.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace WebUI.Data.Models
{
    //[Updatable]
    [RemoveEvent(after: "AfterDelete")]
    [CreateEvent(after: "AfterCreate")]
    [UpdateEvent(after: "AfterUpdate")]
    public class User : Base
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }

        [Required]
        [MinLength(3)]
        [QueryAuth("Test2")]
        [UpdateAuth("Test")]
        [Updatable]
        public string FirstName { get; set; }

        public bool Test(HttpContext context)
        {
            return context.User.IsInRole("admin");
        }

        public bool Test2(HttpContext context, RealtimeContext db)
        {
            return db.Users.Count() > 3;
            //return DateTime.UtcNow.Millisecond % 2 == 0;
        }

        [Required]
        [MinLength(3)]
        [Updatable]
        public string LastName { get; set; }

        public void afterCreate(HttpContext context, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Created user " + Id,
                UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }

        private void AfterUpdate(HttpContext context, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Updated user " + Id,
                UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }

        public void AfterDelete(HttpContext context, RealtimeContext db)
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
