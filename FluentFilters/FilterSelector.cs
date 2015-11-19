using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FluentFilters
{
    public class FilterSelector : DataTemplateSelector
    {
        private readonly IDictionary<Type, DataTemplate> _lookup = new Dictionary<Type, DataTemplate>();

        public DataTemplate Select
        {
            set { _lookup[typeof (SelectFilter<>)] = value; }
        }

        public DataTemplate Boolean
        {
            set { _lookup[typeof (PredicateFilter<>)] = value; }
        }

        public DataTemplate MultiSelect
        {
            set { _lookup[typeof (MultiSelectFilter<>)] = value; }
        }

        public DataTemplate Search
        {
            set { _lookup[typeof (SearchFilter<>)] = value; }
        }

        public DataTemplate Label
        {
            set { _lookup[typeof (FilterBar<>.FilterLabel)] = value; }
        }

        public DataTemplate SelectOption
        {
            set { _lookup[typeof (SelectOptionFilter<>)] = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return base.SelectTemplate(null, container);
            }

            var type = item.GetType();
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            DataTemplate template;
            if (_lookup.TryGetValue(type, out template))
            {
                return template;
            }

            foreach (var kvp in _lookup)
            {
                if (IsSubclassOfRawGeneric(kvp.Key, type))
                {
                    return kvp.Value;
                }
            }

            return base.SelectTemplate(item, container);
        }

        // http://stackoverflow.com/a/457708/114994
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}