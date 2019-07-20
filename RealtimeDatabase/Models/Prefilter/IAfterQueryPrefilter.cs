using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    public interface IAfterQueryPrefilter : IPrefilterBase
    {
        object Execute(IEnumerable<object> array);
    }
}
