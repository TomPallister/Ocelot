using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ocelot.Values
{
    public class UpstreamHeaderTemplate
    {
        public string Template { get; }        

        public string OriginalValue { get; }

        public Regex Pattern { get; }

        public UpstreamHeaderTemplate(string template, string originalValue)
        {
            Template = template;            
            OriginalValue = originalValue;
            Pattern = template == null ?
                new Regex("$^", RegexOptions.Compiled | RegexOptions.Singleline) :
                new Regex(template, RegexOptions.Compiled | RegexOptions.Singleline);
        }
    }
}
