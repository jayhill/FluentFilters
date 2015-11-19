namespace FluentScratchpad
{
    public static class StringExtensions
    {
        public static Contains Includes(this string source, string other)
        {
            return new Contains(source, other);
        }
    }
}