using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esprima.Ast;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SapphireDb.Attributes;
using SapphireDb.Models;
using Expression = System.Linq.Expressions.Expression;

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

        public static Expression<Func<Pixel, bool>> GetQuery()
        {
            return pixel => pixel.X < 5 && pixel.Y < 5;
        }
    }
}
