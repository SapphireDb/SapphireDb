using SapphireDb.Attributes;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using SapphireDb.Models;

namespace WebUI.Data.Models
{
    [CreateEvent("BeforeCreate")]
    [UpdateEvent("BeforeUpdate")]
    public class Message : Base
    {
        public Message()
        {
            CreatedOn = DateTime.UtcNow;
        }

        public void BeforeCreate(HttpInformation context)
        {
            UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value;
        }

        public void BeforeUpdate(HttpInformation context)
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
