using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FluentFilters.Fluent
{
    /// <summary>
    /// Static gateway to our fluent API for creating and configuring filter objects.
    /// </summary>
    public static class Filter
    {
        public static IFilterSelectType<TItem> For<TItem>(string name)
        {
            return new FilterBuilder<TItem>(name);
        }
    }

    public class FilterBuilder<T> : IFilterSelectType<T>
    {
        private readonly string _name;

        public FilterBuilder(string name)
        {
            _name = name;
        }

        ICheckboxFilterBuilder<T> IFilterSelectType<T>.Checkbox
        {
            get { return new CheckBoxFilterBuilder<T>(_name); }
        }

        ISelectFilterBuilder<T> IFilterSelectType<T>.Select
        {
            get { return new SelectFilterBuilder<T>(_name); }
        }

        IConfigureSearch<T> IFilterSelectType<T>.SearchOn(Expression<Func<T,object>> fieldSelector)
        {
            return new SearchFilterBuilder<T>.SearchConfig(new SearchFilterBuilder<T>(_name))
            {
                FieldSelector = fieldSelector
            };
        }
    }

    public interface IFilterSelectType<T>
    {
        ICheckboxFilterBuilder<T> Checkbox { get; }
        ISelectFilterBuilder<T> Select { get; }
        IConfigureSearch<T> SearchOn(Expression<Func<T, object>> fieldSelector);
    }

    public interface ISearchFilterBuilder<T>
    {
        IConfigureSearch<T> SearchOn(Expression<Func<T, object>> fieldSelector);
    }

    public interface ICheckboxFilterBuilder<T>
    {
        FilterBase<T> PassesIf(Predicate<T> predicate);
    }

    public class CheckBoxFilterBuilder<T> : ICheckboxFilterBuilder<T>
    {
        private readonly string _name;

        public CheckBoxFilterBuilder(string name)
        {
            _name = name;
        }

        public FilterBase<T> PassesIf(Predicate<T> predicate)
        {
            return new PredicateFilter<T>(_name, predicate);
        }
    }

    public class SearchFilterBuilder<T> : ISearchFilterBuilder<T>
    {
        private readonly string _name;
        private readonly IList<SearchConfig> _searchConfigs = new List<SearchConfig>();

        public SearchFilterBuilder(string name)
        {
            _name = name;
        }

        public IConfigureSearch<T> SearchOn(Expression<Func<T, object>> fieldSelector)
        {
            return new SearchConfig(this) { FieldSelector = fieldSelector };
        }

        public static implicit operator FilterBase<T>(SearchFilterBuilder<T> f)
        {
            return new SearchFilter<T>(f._name, f._searchConfigs.Cast<SearchFilter<T>.Config>().ToArray());
        }

        internal class SearchConfig : SearchFilter<T>.Config, IConfigureSearch<T>
        {
            private readonly SearchFilterBuilder<T> _parent;

            public SearchConfig(SearchFilterBuilder<T> parent)
            {
                _parent = parent;
                IgnoreCase = true;
            }

            public SearchFilterBuilder<T> StartsWith
            {
                get
                {
                    SearchType = SearchType.StartsWith;
                    _parent._searchConfigs.Add(this);
                    return _parent;
                }
            }

            public SearchFilterBuilder<T> Contains
            {
                get
                {
                    SearchType = SearchType.Contains;
                    _parent._searchConfigs.Add(this);
                    return _parent;
                }
            }
        }
    }

    public interface IConfigureSearch<T>
    {
        SearchFilterBuilder<T> StartsWith { get; }
        SearchFilterBuilder<T> Contains { get; }
    }
}