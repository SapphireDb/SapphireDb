using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RealtimeDatabase.Attributes;

namespace DemoServerApplication.Data.Models
{
    [UpdateEvent("BeforeUpdate")]
    public class Base
    {
        public Base()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTime.UtcNow;
        }

        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public void BeforeUpdate()
        {
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
