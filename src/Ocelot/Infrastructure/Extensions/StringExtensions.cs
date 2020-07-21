using System;

namespace Ocelot.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string TrimStart(this string source, string trim, StringComparison stringComparison = StringComparison.Ordinal)
        {
            if (source == null)
            {
                return null;
            }

            string s = source;
            while (s.StartsWith(trim, stringComparison))
            {
                s = s.Substring(trim.Length);
            }

            return s;
        }

        public static string LastCharAsForwardSlash(this string source)
        {
            if (source.EndsWith('/'))
            {
                return source;
            }

            return $"{source}/";
        }
    }
}
