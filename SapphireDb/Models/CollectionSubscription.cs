using System;
using System.Collections.Generic;
using System.Threading;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models
{
    public class CollectionSubscription : IDisposable
    {
        public string CollectionName { get; set; }

        public string ContextName { get; set; }

        public string ReferenceId { get; set; }

        public List<IPrefilterBase> Prefilters { get; set; }

        public void Dispose()
        {
            Prefilters.ForEach(p => p.Dispose());
        }
    }
}
