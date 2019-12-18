using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace WebUI.Data.DemoDb
{
    public class DemoUser
    {
        [Key]
        public Guid Id { get; set; }

        public List<UserEntry> Entries { get; set; }
    }
}
