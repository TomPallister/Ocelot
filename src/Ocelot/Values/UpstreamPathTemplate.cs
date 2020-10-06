using System;
using System.Text.RegularExpressions;

namespace Ocelot.Values
{
    public class UpstreamPathTemplate
    {
        private static readonly Regex _reg = new Regex("$^", RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
        public UpstreamPathTemplate(string template, int priority, bool containsQueryString, string originalValue)
        {
            Template = template;
            Priority = priority;
            ContainsQueryString = containsQueryString;
            OriginalValue = originalValue;
            Pattern = template == null ?
                _reg :
                new Regex(template, RegexOptions.Compiled | RegexOptions.Singleline, TimeSpan.FromMilliseconds(100));
        }

        public string Template { get; }

        public int Priority { get; }

        public bool ContainsQueryString { get; }

        public string OriginalValue { get; }

        public Regex Pattern { get; }
    }
}
