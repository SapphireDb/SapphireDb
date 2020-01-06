using System;
using System.ComponentModel.DataAnnotations;
using SapphireDb.Attributes;

namespace WebUI.Data.AuthDemo
{
    public class Base
    {
        [Key]
        public int Id { get; set; }
    }
}