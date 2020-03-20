using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Connection;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models
{
    public class CollectionSubscription
    {
        public string ReferenceId { get; set; }
        
        public ConnectionBase Connection { get; set; }
    }
}
