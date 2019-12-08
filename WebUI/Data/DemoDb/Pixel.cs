using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;

namespace WebUI.Data.DemoDb
{
    public class Pixel
    {
        [Key]
        public Guid Id { get; set; }

        [Updatable]
        public string Color { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
