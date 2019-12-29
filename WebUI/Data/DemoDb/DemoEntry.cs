using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    [Updatable]
    public class DemoEntry
    {
        [Key]
        public Guid Id { get; set; }
        
        public string Content { get; set; }
    }
}
