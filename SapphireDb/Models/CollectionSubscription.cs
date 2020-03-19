using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Connection;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models
{
    public class CollectionSubscription : IDisposable
    {
        public string CollectionName { get; set; }

        public string ContextName { get; set; }

        public string ReferenceId { get; set; }

        public string PrefilterHash { get; set; }

        // public bool RequiresDatabaseQuery { get; set; }
     
        public ConnectionBase Connection { get; set; }

        private List<IPrefilterBase> prefilters;
        public List<IPrefilterBase> Prefilters
        {
            get => prefilters;
            set
            {
                prefilters = value;
                PrefilterHash = string.Join(';', prefilters.Select(p => p.Hash()));
                
                // RequiresDatabaseQuery = prefilters.Any(p => p is IAfterQueryPrefilter || p is TakePrefilter ||
                //                     p is SkipPrefilter || p is IncludePrefilter);
                
                // Add hasIncludePrefilterWithChange?
            }
        }
        
        public void Dispose()
        {
            Prefilters.ForEach(p => p.Dispose());
        }
    }
}
