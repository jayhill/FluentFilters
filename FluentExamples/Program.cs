using System;
using System.Text;

namespace FluentScratchpad
{
    class Program
    {
        static void Main()
        {

            string composer = new StringComposer()
                .AppendLine("Hello").Indent(1)
                .AppendLine("World").Indent(2);

            Console.WriteLine(composer);


            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    public static class Extensions
    {
        public static StringBuilder AppendLineIndented(
            this StringBuilder builder, string s, int levels)
        {
            return builder.Append(new string('\t', levels))
                .AppendLine(s);
        }
    }
}
