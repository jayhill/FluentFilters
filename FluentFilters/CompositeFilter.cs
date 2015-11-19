using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FluentFilters
{
    public abstract class CompositeFilter<T,TFilter> : FilterBase<T>, IEnumerable<TFilter> where TFilter : FilterBase<T>
    {
        protected CompositeFilter(string name, params TFilter[] filters) : base(name)
        {
            foreach (var filter in filters)
            {
                Add(filter);
            }
        }

        public override bool Passes(T item)
        {
            return Passes(Filters, item);
        }

        public override bool IsApplied
        {
            get { return Filters.Any(f => f.IsApplied); }
            set { /* do nothing */ }
        }

        protected abstract bool Passes(IEnumerable<TFilter> filters, T item);

        public void Add(TFilter filter)
        {
            _filters.Add(filter);
            filter.IsAppliedChanged += (o, e) => RaiseIsAppliedChanged();
        }

        private readonly IList<TFilter> _filters = new List<TFilter>();
        public virtual IEnumerable<TFilter> Filters
        {
            get { return _filters; }
        }

        public IEnumerator<TFilter> GetEnumerator()
        {
            return Filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}