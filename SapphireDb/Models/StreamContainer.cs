using System;

namespace SapphireDb.Models
{
    public class StreamContainer
    {
        public object AsyncEnumerable { get; set; }

        public Guid ConnectionId { get; set; }

        public DateTime LastFrame { get; set; }

        public Type EnumerableType { get; set; }
    }
}