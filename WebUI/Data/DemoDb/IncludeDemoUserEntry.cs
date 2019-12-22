using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    public class IncludeDemoUserEntry
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }

        public IncludeDemoUser User { get; set; }

        [Updatable]
        public string Content { get; set; }
    }
}
