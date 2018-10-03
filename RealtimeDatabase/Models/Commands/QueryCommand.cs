using RealtimeDatabase.Models.Prefilter;
using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class QueryCommand : CollectionCommandBase
    {
        public List<IPrefilter> Prefilters { get; set; }
    }
}
