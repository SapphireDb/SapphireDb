using RealtimeDatabase.Attributes;
using RealtimeDatabase.Websocket.Models;
using System;
using System.Linq;

namespace WebUI.Data.Models
{
    [QueryAuth("Auth")]
    public class Message : Base
    {
        public Message()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public bool Auth(WebsocketConnection websocketConnection)
        {
            string userId = websocketConnection.HttpContext.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value;
            return !string.IsNullOrEmpty(userId) && (UserId == userId || ToId == userId);
        }

        public void OnCreate(WebsocketConnection websocketConnection)
        {
            UserId = websocketConnection.HttpContext.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value;
        }

        public void OnUpdate(WebsocketConnection websocketConnection)
        {
            UpdatedOn = DateTime.UtcNow;
        }

        public string UserId { get; set; }

        public string ToId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
