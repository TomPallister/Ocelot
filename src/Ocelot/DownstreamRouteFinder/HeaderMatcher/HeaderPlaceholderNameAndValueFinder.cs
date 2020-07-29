using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ocelot.DownstreamRouteFinder.HeaderMatcher
{
    public class HeaderPlaceholderNameAndValueFinder : IHeaderPlaceholderNameAndValueFinder
    {
        public List<PlaceholderNameAndValue> Find(Dictionary<string, string> upstreamHeaders, Dictionary<string, UpstreamHeaderTemplate> templateHeaders)
        {
            var placeholderNameAndValuesList = new List<PlaceholderNameAndValue>();

            foreach (var templateHeader in templateHeaders)
            {
                var upstreamHeader = upstreamHeaders[templateHeader.Key];
                var matches = templateHeader.Value.Pattern.Matches(upstreamHeader);
                var placeholders = matches.SelectMany(g => g.Groups as IEnumerable<Group>).Where(g => g.Name != "0")
                                          .Select(g => new PlaceholderNameAndValue("{"+g.Name+"}", g.Value));
                placeholderNameAndValuesList.AddRange(placeholders);
            }

            return placeholderNameAndValuesList;
        }
    }
}
