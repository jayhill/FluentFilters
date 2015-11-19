using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentFilters
{
    public static class CompositeFilterStrategy
    {
        public static Func<IEnumerable<FilterBase<T>>, Predicate<T>> PassesAny<T>()
        {
            return filters => t => filters.Any(f => f.Passes(t));
        }

        public static Func<IEnumerable<FilterBase<T>>, Predicate<T>> PassesAll<T>()
        {
            return filters => t => filters.All(f => f.Passes(t));
        }
    }
}