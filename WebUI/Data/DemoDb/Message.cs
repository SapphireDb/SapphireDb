using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RealtimeDatabase.Attributes;

namespace WebUI.Data.DemoDb
{
    public class Message
    {
        public Message()
        {
            CreatedOn = DateTime.UtcNow;
        }

        [Key]
        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Content { get; set; }
    }
}
