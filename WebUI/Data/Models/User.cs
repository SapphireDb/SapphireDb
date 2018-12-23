using Microsoft.AspNetCore.Authorization;
using RealtimeDatabase;
using RealtimeDatabase.Attributes;
using RealtimeDatabase.Models;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        [QueryAuth("test2")]
        [UpdateAuth("test")]
        [Updatable]
        public string FirstName { get; set; }

        public bool test(WebsocketConnection connection)
        {
            return connection.HttpContext.User.IsInRole("admin");
        }

        public bool test2(WebsocketConnection connection)
        {
            return DateTime.UtcNow.Millisecond % 2 == 0;
        }

        [Required]
        [MinLength(3)]
        [Updatable]
        public string LastName { get; set; }

        public void OnCreate(WebsocketConnection websocketConnection)
        {

        }

        public void OnUpdate(WebsocketConnection websocketConnection)
        {

        }
    }
}
