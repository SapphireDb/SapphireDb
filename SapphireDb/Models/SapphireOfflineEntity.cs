using System;
using System.ComponentModel.DataAnnotations;

namespace SapphireDb.Models
{
    public class SapphireOfflineEntity
    {
        public SapphireOfflineEntity()
        {
            Id = Guid.NewGuid();
        }
        
        [Key]
        public Guid Id { get; set; }

        public DateTime ModifiedOn { get; set; }

        public void MarkModified()
        {
            ModifiedOn = DateTime.UtcNow;
        }
    }
}