using RealtimeDatabase.Models.Prefilter;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models
{
    public class CollectionSubscription
    {
        public string CollectionName { get; set; }

        public string ReferenceId { get; set; }

        public List<IPrefilter> Prefilters { get; set; }

        public List<object[]> TransmittedData { get; set; }
    }
}
