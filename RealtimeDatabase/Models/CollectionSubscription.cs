using System;
using RealtimeDatabase.Models.Prefilter;
using System.Collections.Generic;
using System.Threading;

namespace RealtimeDatabase.Models
{
    public class CollectionSubscription : IDisposable
    {
        public string CollectionName { get; set; }

        public string ReferenceId { get; set; }

        public List<IPrefilterBase> Prefilters { get; set; }

        public List<object[]> TransmittedData { get; set; }

        public SemaphoreSlim Lock = new SemaphoreSlim(1, 1);

        public void Dispose()
        {
            Prefilters.ForEach(p => p.Dispose());
        }
    }
}
