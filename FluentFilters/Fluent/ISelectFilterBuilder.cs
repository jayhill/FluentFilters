using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentFilters.Fluent
{
    public interface ISelectFilterBuilder<T>
    {
        ISelectFilterRequiresData<T> On(Expression<Func<T, object>> fieldSelector);
        SelectFilterBuilder<T>.SelectRangeFilterOptions RangeOn(Expression<Func<T, object>> fieldSelector);
    }

    public interface ISelectFilterRequiresData<T>
    {
        SelectFilterBuilder<T>.SelectFilterOptions WithDataFrom(IEnumerable<T> data);
    }

    public class SelectFilterBuilder<T> : ISelectFilterBuilder<T>, ISelectFilterRequiresData<T>
    {
        private readonly string _name;
        private Expression<Func<T, object>> _fieldSelector;

        public SelectFilterBuilder(string name)
        {
            _name = name;
        }
        public ISelectFilterRequiresData<T> On(Expression<Func<T, object>> fieldSelector)
        {
            _fieldSelector = fieldSelector;
            return this;
        }

        SelectFilterOptions ISelectFilterRequiresData<T>.WithDataFrom(IEnumerable<T> data)
        {
            return new SelectFilterOptions(_name, data, _fieldSelector);
        }

        public SelectRangeFilterOptions RangeOn(Expression<Func<T, object>> fieldSelector)
        {
            return new SelectRangeFilterOptions(_name, fieldSelector);
        }

        public class SelectFilterOptions
        {
            private readonly string _name;
            private readonly IEnumerable<T> _data;
            private readonly Expression<Func<T, object>> _fieldSelector;
            private bool _reverse;

            public SelectFilterOptions(string name, IEnumerable<T> data, Expression<Func<T, object>> fieldSelector)
            {
                _name = name;
                _data = data;
                _fieldSelector = fieldSelector;
            }

            public FilterBase<T> OrderDesc
            {
                get
                {
                    _reverse = true;
                    return this;
                }
            }

            public FilterBase<T> OrderAsc
            {
                get { return this; }
            }

            public static implicit operator FilterBase<T>(SelectFilterOptions builder)
            {
                var options = builder._data.Select(x => builder._fieldSelector.GetValue(x).ToString()).Distinct()
                    .Select(x => new SelectOptionFilter<T>(x, y => builder._fieldSelector.GetValue(y).ToString().Equals(x, StringComparison.OrdinalIgnoreCase))); 

                options = builder._reverse 
                    ? options.OrderByDescending(x => x.Name)
                    : options.OrderBy(x => x.Name);

                return new SelectFilter<T>(builder._name, options.Cast<FilterBase<T>>().ToArray());
            }

        }

        public class SelectRangeFilterOptions
        {
            private readonly string _name;
            private readonly Expression<Func<T, object>> _fieldSelector;
            private readonly IList<RangeDefinition> _ranges = new List<RangeDefinition>();

            public SelectRangeFilterOptions(string name, Expression<Func<T,object>> fieldSelector)
            {
                _name = name;
                _fieldSelector = fieldSelector;
            }

            public SelectRangeFilterOptions AddRange(string label, int lower, int upper)
            {
                _ranges.Add(new RangeDefinition(label, lower, upper));
                return this;
            }

            public static implicit operator FilterBase<T>(SelectRangeFilterOptions builder)
            {
                var options = builder._ranges.Select(range =>
                {
                    return new SelectOptionFilter<T>(range.Name, x =>
                    {
                        var value = (int) builder._fieldSelector.GetValue(x);
                        return value >= range.Lower && value < range.Upper;
                    });
                }).Cast<FilterBase<T>>().ToArray();
                return new SelectFilter<T>(builder._name, options);
            }
        }

        private class RangeDefinition
        {
            public RangeDefinition(string name, int lower, int upper)
            {
                Name = name;
                Lower = lower;
                Upper = upper;
            }

            public string Name { get; set; }
            public int Lower { get; set; }
            public int Upper { get; set; }
        }
    }

}