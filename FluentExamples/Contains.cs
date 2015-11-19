using System;
using System.Linq;

namespace FluentScratchpad
{
    public class Contains
    {
        private readonly string _source;
        private string _other;
        private bool _ignoreCase;

        public Contains(string source, string other)
        {
            _source = source;
            _other = other;
        }

        public Contains IgnoreCase
        {
            get
            {
                _ignoreCase = true;
                return this;
            }
        }

        public Contains Backward
        {
            get
            {
                _other = new string(_other.Reverse().ToArray());
                return this;
            }
        }

        private bool Evaluate()
        {
            return _source.IndexOf(_other, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}