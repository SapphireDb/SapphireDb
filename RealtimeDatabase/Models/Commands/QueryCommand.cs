using RealtimeDatabase.Models.Prefilter;
using System.Collections.Generic;

namespace RealtimeDatabase.Models.Commands
{
    public class QueryCommand : CollectionCommandBase
    {
        public List<IPrefilterBase> Prefilters { get; set; } = new List<IPrefilterBase>();
    }
}
