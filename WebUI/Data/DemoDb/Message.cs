using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    public class Message
    {
        public Message()
        {
            CreatedOn = DateTimeOffset.UtcNow;
        }

        [Key]
        public Guid Id { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Content { get; set; }
    }
}
