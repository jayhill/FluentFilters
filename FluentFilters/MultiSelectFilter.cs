using System.Collections.Generic;
using System.Linq;

namespace FluentFilters
{
    public class MultiSelectFilter<T> : CompositeFilter<T, FilterBase<T>>
    {
        public MultiSelectFilter(string name, params FilterBase<T>[] filters) : base(name, filters) { }

        protected override bool Passes(IEnumerable<FilterBase<T>> filters, T item)
        {
            return CompositeFilterStrategy.PassesAll<T>()(filters.Where(f => f.IsApplied))(item);
        }
    }
}