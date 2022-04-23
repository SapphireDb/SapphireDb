using System;
using System.ComponentModel.DataAnnotations;
using SapphireDb.Attributes;

namespace SapphireDb.Models
{
    [UpdateEvent(BeforeSave = nameof(MarkModified))]
    public class SapphireOfflineEntity
    {
        public SapphireOfflineEntity()
        {
            Id = Guid.NewGuid();
            ModifiedOn = DateTimeOffset.UtcNow;
        }
        
        [Key]
        public Guid Id { get; set; }

        [ConcurrencyCheck]
        public DateTimeOffset ModifiedOn { get; set; }

        public void MarkModified()
        {
            ModifiedOn = DateTimeOffset.UtcNow;
        }
    }
}