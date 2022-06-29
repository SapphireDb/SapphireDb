using System;
using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public class FirstPrefilter : IPrefilter
    {
        public IQueryable<object> Execute(IQueryable<object> array)
        {
            return array.Take(1);
        }

        public void Dispose()
        {
            
        }

        public void Initialize(Type modelType, IServiceProvider serviceProvider) { } 
        
        public string Hash()
        {
            return "FirstPrefilter";
        }
    }
}
