using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

namespace FluentFilters
{
    public class FilterBar<T> : CompositeFilter<T, FilterBase<T>>
    {
        private readonly IList<T> _items;
        private readonly ListCollectionView _collectionView;
        private readonly FilterLabel _label;

        public FilterBar(string name, IList<T> items, params FilterBase<T>[] filters) : base(name, filters)
        {
            _items = items;
            _collectionView = (ListCollectionView) CollectionViewSource.GetDefaultView(items);
            _label = new FilterLabel(() => _items.Count, () => _collectionView.Count);
        }

        public override bool IsApplied
        {
            get { return base.IsApplied; }
            set
            {
                base.IsApplied = value;
                if (!value)
                {
                    foreach (var filter in Filters)
                    {
                        filter.IsApplied = false;
                    }
                }
            }
        }

        protected override void RaiseIsAppliedChanged()
        {
            base.RaiseIsAppliedChanged();
            _collectionView.Filter = x =>
            {
                if (x is T)
                {
                    return Passes((T) x);
                }

                return true;
            };

            _label.Update();
        }

        protected override bool Passes(IEnumerable<FilterBase<T>> filters, T item)
        {
            return !IsApplied || CompositeFilterStrategy.PassesAll<T>()(filters)(item);
        }

        public IEnumerable<object> Items
        {
            get
            {
                foreach (var filter in Filters)
                {
                    yield return filter;
                }

                yield return _label;
            }
        }

        public class FilterLabel : INotifyPropertyChanged
        {
            private readonly Func<int> _getTotalCount;
            private readonly Func<int> _getVisibleCount;

            public FilterLabel(Func<int> getTotalCount, Func<int> getVisibleCount)
            {
                _getTotalCount = getTotalCount;
                _getVisibleCount = getVisibleCount;
                Update();
            }

            public string Text { get; private set; }

            public void Update()
            {
                var text = Text;
                var total = _getTotalCount();
                var visible = _getVisibleCount();

                var item = total == 1 ? "item" : "items";

                if (total == 0)
                {
                    Text = "No items";
                }
                else if (total == visible)
                {
                    Text = string.Format("Showing all {0} {1}", total, item);
                }
                else
                {
                    Text = string.Format("Showing {0} of {1} {2}", visible, total, item);
                }

                if (Text != text)
                {
                    var handler = PropertyChanged;
                    if (handler != null)
                    {
                        handler(this, new PropertyChangedEventArgs("Text"));
                    }
                }
            }

            public override string ToString()
            {
                return Text;
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}