using System.ComponentModel.DataAnnotations;

namespace WebUI.Data.AuthDemo
{
    public class Base
    {
        [Key]
        public int Id { get; set; }
    }
}