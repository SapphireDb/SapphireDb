using System.ComponentModel.DataAnnotations;
using RealtimeDatabase.Attributes;

namespace WebUI.Data.Models
{
    public class Base
    {
        [Key]
        public int Id { get; set; }
    }
}
