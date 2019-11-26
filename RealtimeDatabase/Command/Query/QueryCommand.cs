using System.Collections.Generic;
using RealtimeDatabase.Internal.Prefilter;

namespace RealtimeDatabase.Command.Query
{
    public class QueryCommand : CollectionCommandBase
    {
        public List<IPrefilterBase> Prefilters { get; set; } = new List<IPrefilterBase>();
    }
}
