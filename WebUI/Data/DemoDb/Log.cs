using System;
using System.ComponentModel.DataAnnotations;

namespace WebUI.Data.DemoDb
{
    public class Log
    {
        [Key]
        public Guid Id { get; set; }

        public string Content { get; set; }
    }
}