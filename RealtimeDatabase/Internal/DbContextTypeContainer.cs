using System;
using System.Collections.Generic;

namespace RealtimeDatabase.Internal
{
    public class DbContextTypeContainer
    {
        public Dictionary<string, Type> DbContextTypes { get; set; } = new Dictionary<string, Type>();
    }
}
