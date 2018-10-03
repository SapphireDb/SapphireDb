using System;
using System.Collections.Generic;
using System.Text;

namespace RealtimeDatabase.Models.Prefilter
{
    public interface IPrefilter
    {
        IEnumerable<object> Execute(IEnumerable<object> array);
    }
}
