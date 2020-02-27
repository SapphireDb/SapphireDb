using System;
using System.Collections.Generic;
using System.Linq;


namespace SapphireDb.Internal.Prefilter
{
    public class FirstPrefilter : IAfterQueryPrefilter
    {
        public object Execute(IQueryable<object> array)
        {
            return array.FirstOrDefault();
        }

        public void Dispose()
        {
            
        }

        public void Initialize(Type modelType) { } 
        
        public string Hash()
        {
            return "FirstPrefilter";
        }
    }
}
