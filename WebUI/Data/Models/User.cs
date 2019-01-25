using RealtimeDatabase.Attributes;
using RealtimeDatabase.Websocket.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WebUI.Data.Models
{
    //[Updatable]
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

        public bool Test(WebsocketConnection connection)
        {
            return connection.HttpContext.User.IsInRole("admin");
        }

        public bool Test2(WebsocketConnection connection, RealtimeContext db)
        {
            return db.Users.Count() > 3;
            //return DateTime.UtcNow.Millisecond % 2 == 0;
        }

        [Required]
        [MinLength(3)]
        [Updatable]
        public string LastName { get; set; }

        public void OnCreate(WebsocketConnection websocketConnection, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Created user " + Id,
                UserId = websocketConnection.HttpContext.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }

        public void OnUpdate(WebsocketConnection websocketConnection, RealtimeContext db)
        {
            db.Logs.Add(new Log()
            {
                Message = "Updated user " + Id,
                UserId = websocketConnection.HttpContext.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value
            });
            db.SaveChanges();
        }
    }
}
