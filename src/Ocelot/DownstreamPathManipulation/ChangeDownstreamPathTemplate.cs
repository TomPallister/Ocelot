using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Infrastructure;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;
using Ocelot.Values;

namespace Ocelot.PathManipulation
{
    public class ChangeDownstreamPathTemplate : IChangeDownstreamPathTemplate
    {
        private readonly IClaimsParser _claimsParser;

        public ChangeDownstreamPathTemplate(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response ChangeDownstreamPath(List<ClaimToThing> claimsToThings, IEnumerable<Claim> claims,
            DownstreamPathTemplate downstreamPathTemplate, List<PlaceholderNameAndValue> placeholders)
        {
            foreach (var config in claimsToThings)
            {
                var value = _claimsParser.GetValue(claims, config.NewKey, config.Delimiter, config.Index);

                if (value.IsError)
                {
                    return new ErrorResponse(value.Errors);
                }

                var placeholderName = $"{{{config.ExistingKey}}}";

                if (!downstreamPathTemplate.Value.Contains(placeholderName))
                {
                    return new ErrorResponse(new CouldNotFindPlaceholderError(placeholderName));
                }

                if (placeholders.Any(ph => ph.Name == placeholderName))
                {
                    placeholders.RemoveAll(ph => ph.Name == placeholderName);
                }

                placeholders.Add(new PlaceholderNameAndValue(placeholderName, value.Data));
            }

            return new OkResponse();
        }
    }
}
