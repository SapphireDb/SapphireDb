using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RealtimeDatabase.Attributes;

namespace WebUI.Data.DemoDb
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Updatable]
        public string Content { get; set; }
    }
}
