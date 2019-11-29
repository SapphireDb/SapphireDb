using System.Collections.Generic;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Command.Query
{
    public class QueryCommand : CollectionCommandBase
    {
        public List<IPrefilterBase> Prefilters { get; set; } = new List<IPrefilterBase>();
    }
}
