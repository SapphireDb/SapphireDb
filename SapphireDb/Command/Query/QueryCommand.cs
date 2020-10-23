using System.Collections.Generic;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Command.Query
{
    public class QueryCommand : CollectionCommandBase, IQueryCommand
    {
        public List<IPrefilterBase> Prefilters { get; } = new List<IPrefilterBase>();
    }
}
