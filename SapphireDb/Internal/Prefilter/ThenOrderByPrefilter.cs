using System;
using System.Collections.Generic;
using System.Linq;
using JavaScriptEngineSwitcher.Core;
using SapphireDb.Helper;

// ReSharper disable PossibleMultipleEnumeration

namespace SapphireDb.Internal.Prefilter
{
    public class ThenOrderByPrefilter : OrderByPrefilter
    {
        public IQueryable<object> Execute(IOrderedQueryable<object> array)
        {
            return Descending
                ? array.ThenByDescending(PropertySelectExpression)
                : array.ThenBy(PropertySelectExpression);
        }
    }
}
