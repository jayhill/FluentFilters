using System;
using System.ComponentModel;

namespace FluentFilters
{
    public abstract class FilterBase : INotifyPropertyChanged
    {
        protected FilterBase(string name)
        {
            Name = name;
        }

        public virtual string Name { get; private set; }

        private bool _isApplied;
        public virtual bool IsApplied
        {
            get { return _isApplied; }
            set
            {
                var wasApplied = _isApplied;
                _isApplied = value;
                if (_isApplied != wasApplied)
                {
                    RaiseIsAppliedChanged();
                }
            }
        }

        public event EventHandler IsAppliedChanged;

        protected virtual void RaiseIsAppliedChanged()
        {
            var handler = IsAppliedChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            RaisePropertyChanged("IsApplied");
        }

        protected void RaisePropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public abstract class FilterBase<T> : FilterBase
    {
        public abstract bool Passes(T item);

        protected FilterBase(string name) : base(name) { }
    }
}