namespace FluentFilters
{
    public class NoSelectionFilter<T> : SelectOptionFilter<T>
    {
        public NoSelectionFilter() : base(string.Empty, t => true) { }

        public override bool IsApplied
        {
            get { return false; }
            set { /* do nothing */ }
        }
    }
}