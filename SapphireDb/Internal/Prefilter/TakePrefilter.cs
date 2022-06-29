using System;
using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public class TakePrefilter : IPrefilter
    {
        public int Number { get; set; }

        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Take(Number);
        }

        public void Initialize(Type modelType, IServiceProvider serviceProvider) { }

        public void Dispose()
        {
            
        }
        
        public string Hash()
        {
            return $"TakePrefilter,{Number}";
        }
    }
}
