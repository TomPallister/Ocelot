using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using Ocelot.Values;
using System.Collections.Generic;
using System.Text;

namespace Ocelot.DownstreamUrlCreator.UrlTemplateReplacer
{
    public class DownstreamTemplatePathPlaceholderReplacer : IDownstreamPathPlaceholderReplacer
    {
        public Response<DownstreamPath> Replace(string downstreamPathTemplate,
            List<PlaceholderNameAndValue> urlPathPlaceholderNameAndValues)
        {
            var downstreamPath = new StringBuilder();

            downstreamPath.Append(downstreamPathTemplate);

            foreach (var placeholderVariableAndValue in urlPathPlaceholderNameAndValues)
            {
                downstreamPath.Replace(placeholderVariableAndValue.Name, placeholderVariableAndValue.Value);
            }

            return new OkResponse<DownstreamPath>(new DownstreamPath(downstreamPath.ToString()));
        }
    }
}
