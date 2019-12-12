using System;
using System.Collections.Generic;
using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public class TakePrefilter : IPrefilter
    {
        public int Number { get; set; }

        public bool Initialized { get; set; } = true;

        public void Initialize(Type modelType) { }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Take(Number);
        }

        public void Dispose()
        {
            
        }
    }
}
