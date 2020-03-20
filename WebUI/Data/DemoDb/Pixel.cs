using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace WebUI.Data.DemoDb
{
    //[QueryFunction("GetQuery")]
    public class Pixel
    {
        [Key]
        public Guid Id { get; set; }

        [Updatable]
        public string Color { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        // public static Expression<Func<Pixel, bool>> GetQuery()
        // {
        //     return pixel => pixel.X < 5 && pixel.Y < 5;
        // }
    }
}
