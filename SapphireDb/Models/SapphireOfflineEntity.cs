using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    [UpdateEvent(BeforeSave = nameof(MarkModified))]
    public class SapphireOfflineEntity
    {
        public SapphireOfflineEntity()
        {
            Id = Guid.NewGuid();
            ModifiedOn = DateTime.UtcNow;
        }
        
        [Key]
        public Guid Id { get; set; }

        [ConcurrencyCheck]
        public DateTime ModifiedOn { get; set; }

        public void MarkModified()
        {
            ModifiedOn = DateTime.UtcNow;
        }
    }
}