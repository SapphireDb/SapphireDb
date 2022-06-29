using System;
using System.Collections.Generic;
using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public class SkipPrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Skip(Number);
        }

        public void Initialize(Type modelType, IServiceProvider serviceProvider) { }

        public void Dispose()
        {
            
        }
        
        public string Hash()
        {
            return $"SkipPrefilter,{Number}";
        }
    }
}
