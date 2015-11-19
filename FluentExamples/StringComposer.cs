using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentScratchpad
{
    public interface ICanIndent
    {
        StringComposer Indent();
        StringComposer Indent(int levels);
        ICanIndent Append(string s);
        ICanIndent AppendLine(string s);
    }

    public class StringComposer : ICanIndent
    {
        private IList<Composition> _compositions = new List<Composition>();

        private class Composition
        {
            public int Indentation { get; set; }
            public string Value { get; set; }
        }

        StringComposer ICanIndent.Indent(int levels)
        {
            _compositions.Last().Indentation += levels;
            return this;
        }

        StringComposer ICanIndent.Indent()
        {
            return ((ICanIndent)this).Indent(1);
        }

        public ICanIndent AppendLine(string s)
        {
            _compositions.Add(new Composition{Value = s + Environment.NewLine});
            return this;
        }

        public ICanIndent Append(string s)
        {
            _compositions.Add(new Composition{Value = s});
            return this;
        }

        public static implicit operator String(StringComposer c)
        {
            return c.MakeString();
        }

        private string MakeString()
        {
            return _compositions
                .Select(comp => new string('\t', comp.Indentation)
                                + comp.Value)
                .Aggregate(string.Empty, (s1, s2) => s1 + s2);

        }
    }
}