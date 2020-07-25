using System;

namespace SapphireDb.Models
{
    public class ExecutionContext
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}