using Ocelot.Configuration.File;
using Ocelot.Values;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ocelot.Configuration.Creator
{
    public class UpstreamHeaderTemplatePatternCreator : IUpstreamHeaderTemplatePatternCreator
    {
        private const string RegExMatchOneOrMoreOfEverything = ".+";
        private const string RegExMatchOneOrMoreOfEverythingUntilNextForwardSlash = "[^/]+";
        private const string RegExMatchEndString = "$";
        private const string RegExIgnoreCase = "(?i)";
        private const string RegExForwardSlashOnly = "^/$";
        private const string RegExForwardSlashAndOnePlaceHolder = "^/.*";
        private const string RegExPlaceholders = @"\{(.*?)\}";

        public Dictionary<string, UpstreamHeaderTemplate> Create(IRoute route)
        {
            var resultHeaderTemplates = new Dictionary<string, UpstreamHeaderTemplate>();

            foreach (var headerTemplate in route.UpstreamHeaderTemplates)
            {
                var headerTemplateValue = headerTemplate.Value;

                var placeholders = new List<string>();

                Regex expression = new Regex(RegExPlaceholders);
                MatchCollection matches = expression.Matches(headerTemplateValue);
                if (matches.Count > 0)
                {
                    placeholders.AddRange(matches.Select(m => m.Groups[1].Value));               
                }

                for (int i = 0; i < placeholders.Count; i++)
                {
                    var indexOfPlaceholder = headerTemplateValue.IndexOf(placeholders[i]);

                    headerTemplateValue = headerTemplateValue.Replace(placeholders[i], RegExMatchOneOrMoreOfEverything);                   
                }

                var template = route.RouteIsCaseSensitive
                ? $"^{headerTemplateValue}{RegExMatchEndString}"
                : $"^{RegExIgnoreCase}{headerTemplateValue}{RegExMatchEndString}";

                resultHeaderTemplates.Add(headerTemplate.Key, new UpstreamHeaderTemplate(template, headerTemplate.Value));
            }

            return resultHeaderTemplates;
        }        
    }
}
