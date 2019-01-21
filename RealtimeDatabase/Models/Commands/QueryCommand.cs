using RealtimeDatabase.Models.Prefilter;
using System.Collections.Generic;

namespace RealtimeDatabase.Models.Commands
{
    class QueryCommand : CollectionCommandBase
    {
        public List<IPrefilter> Prefilters { get; set; }
    }
}
