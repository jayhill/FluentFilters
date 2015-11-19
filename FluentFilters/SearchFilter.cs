using System;
using System.Linq.Expressions;
using System.Timers;
using System.Windows;

namespace FluentFilters
{
    public class SearchFilter<T> : FilterBase<T>
    {
        private readonly Config[] _configs;
        private string _searchTerm;
        private readonly Timer _searchPauseTimer = new Timer(500);

        public SearchFilter(string name, params Config[] configs) : base(name)
        {
            _configs = configs;

            // Execute a search after the user pauses so we don't re-filter on every keystroke
            _searchPauseTimer.Elapsed += (sender, args) =>
            {
                _searchPauseTimer.Stop();
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RaiseIsAppliedChanged();
                    RaisePropertyChanged("SearchTerm");
                });
            };
        }

        public override bool Passes(T item)
        {
            if (!IsApplied)
            {
                return true;
            }

            foreach (var config in _configs)
            {
                var value = config.FieldSelector.GetValue(item).ToString();
                var comparison = config.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                switch (config.SearchType)
                {
                    case SearchType.StartsWith:
                        if (value.StartsWith(SearchTerm, comparison)) return true;
                        break;
                    case SearchType.Contains:
                        if (value.IndexOf(SearchTerm, comparison) >= 0) return true;
                        break;
                }
            }

            return false;
        }

        public override bool IsApplied
        {
            get { return !String.IsNullOrEmpty(SearchTerm); }
            set
            {
                if (!value)
                {
                    SearchTerm = string.Empty;
                }
            }
        }

        public string SearchTerm
        {
            get { return _searchTerm; }
            set
            {
                if (_searchPauseTimer.Enabled)
                {
                    _searchPauseTimer.Stop();
                }
                _searchPauseTimer.Start();
                _searchTerm = value;
            }
        }

        public class Config
        {
            public Config()
            {
                IgnoreCase = true;
            }

            public Expression<Func<T,object>> FieldSelector { get; set; }
            public SearchType SearchType { get; set; }
            public bool IgnoreCase { get; set; }
        }
    }

    public enum SearchType
    {
        Contains,
        StartsWith
    }
}