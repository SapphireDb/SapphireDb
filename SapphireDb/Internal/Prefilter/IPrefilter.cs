using System;
using System.Collections.Generic;
using System.Linq;

namespace SapphireDb.Internal.Prefilter
{
    public interface IPrefilter : IPrefilterBase
    {
        bool Initialized { get; set; }
        void Initialize(Type modelType);

        IQueryable<object> Execute(IQueryable<object> array);
    }
}
