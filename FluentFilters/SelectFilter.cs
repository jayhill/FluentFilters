using System.Collections.Generic;

namespace FluentFilters
{
    public class SelectFilter<T> : CompositeFilter<T, FilterBase<T>>
    {
        private readonly FilterBase<T> _noSelectionFilter = new NoSelectionFilter<T>();

        public SelectFilter(string name, params FilterBase<T>[] options) : base(name)
        {
            Add(_noSelectionFilter);
            options.ForEach(Add);
            SelectedFilter = _noSelectionFilter;
        }

        private FilterBase<T> _selectedFilter;
        public FilterBase<T> SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                if (_selectedFilter != null)
                {
                    _selectedFilter.IsApplied = false;
                }
                _selectedFilter = value;
                if (value != null)
                {
                    value.IsApplied = true;
                }
                RaiseIsAppliedChanged();
                RaisePropertyChanged("SelectedFilter");
            }
        }

        public override bool IsApplied
        {
            get { return SelectedFilter != null && SelectedFilter.IsApplied; }
            set
            {
                if (!value)
                {
                    SelectedFilter = _noSelectionFilter;
                }
            }
        }

        public override bool Passes(T item)
        {
            return SelectedFilter == null || SelectedFilter.Passes(item);
        }

        protected override bool Passes(IEnumerable<FilterBase<T>> filters, T item)
        {
            return Passes(item);
        }
    }
}