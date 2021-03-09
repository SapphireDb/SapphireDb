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
            CreatedOn = DateTimeOffset.UtcNow;
        }

        public void BeforeCreate(HttpInformation context)
        {
            UserId = context.User.Claims.FirstOrDefault(cl => cl.Type == "Id")?.Value;
        }

        public void BeforeUpdate(HttpInformation context)
        {
            UpdatedOn = DateTimeOffset.UtcNow;
        }

        public string UserId { get; set; }

        public string ToId { get; set; }

        public string Content { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }
    }
}
