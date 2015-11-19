using System;

namespace FluentFilters
{
    public class SelectOptionFilter<T> : PredicateFilter<T>
    {
        public SelectOptionFilter(string name, Predicate<T> predicate) : base(name, predicate) { }
    }
}