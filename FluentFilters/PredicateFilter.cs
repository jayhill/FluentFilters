using System;

namespace FluentFilters
{
    public class PredicateFilter<T> : FilterBase<T>
    {
        private readonly Predicate<T> _predicate;

        public PredicateFilter(string name, Predicate<T> predicate) : base(name)
        {
            _predicate = predicate;
        }

        public override bool Passes(T item)
        {
            return !IsApplied || _predicate(item);
        }
    }
}