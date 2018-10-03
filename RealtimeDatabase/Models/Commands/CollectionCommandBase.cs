using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Commands
{
    class CollectionCommandBase : CommandBase
    {
        public string CollectionName { get; set; }
    }
}
