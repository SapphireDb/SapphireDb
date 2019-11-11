using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RealtimeDatabase.Attributes;

namespace WebUI.Data.DemoDb
{
    [Updatable]
    public class DemoEntry
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; }
    }
}
