using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SapphireDb.Helper;
using SapphireDb.Internal.Prefilter;

namespace SapphireDb.Models
{
    public class PrefilterContainer : IDisposable
    {
        public PrefilterContainer(List<IPrefilterBase> prefilters)
        {
            Prefilters = prefilters;
        }
        
        private List<IPrefilterBase> prefilters;

        public List<IPrefilterBase> Prefilters
        {
            get => prefilters;
            private set
            {
                prefilters = value;
                Hash = string.Join(';', prefilters.Select(p => p.Hash())).ComputeHash();
            }
        }

        private string Hash { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is PrefilterContainer prefilterContainer)
            {
                return prefilterContainer.Hash == Hash;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Hash.GetHashCode();
        }

        public void Dispose()
        {
            Prefilters.ForEach(p => p.Dispose());
        }
        
        public bool HasAffectedIncludePrefilter(string collectionName)
        {
            return Prefilters.Any(p =>
                p is IncludePrefilter includePrefilter &&
                includePrefilter.AffectedCollectionNames.Any(n =>
                    n.Equals(collectionName, StringComparison.InvariantCultureIgnoreCase))
            );
        }

        private readonly SemaphoreSlim handleLock = new SemaphoreSlim(1);
        public Guid LastHandlingId { get; set; }

        public bool StartHandling(Guid handlingId)
        {
            handleLock.Wait();

            if (LastHandlingId == handlingId)
            {
                return false;
            }
            
            LastHandlingId = handlingId;

            return true;
        }

        public void FinishHandling()
        {
            handleLock.Release();
        }
    }
}