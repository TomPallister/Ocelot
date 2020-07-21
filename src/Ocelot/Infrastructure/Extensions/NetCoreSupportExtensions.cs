using System;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.Infrastructure.Extensions
{
    /// <summary>
    /// Trivial implementations of methods present in .NET Core 2 but not supported on .NET Standard 2.0.
    /// </summary>
    internal static class NetCoreSupportExtensions
    {
        internal static void AppendJoin<T>(this StringBuilder builder, char separator, IEnumerable<T> values)
        {
            builder.Append(string.Join(separator.ToString(), values));
        }

        internal static string[] Split(this string input, string separator, StringSplitOptions options = StringSplitOptions.None)
        {
            return input.Split(new[] { separator }, options);
        }

        internal static bool StartsWith(this string input, char value)
        {
            return input.StartsWith(value.ToString());
        }

        internal static bool EndsWith(this string input, char value)
        {
            return input.EndsWith(value.ToString());
        }
    }
}
