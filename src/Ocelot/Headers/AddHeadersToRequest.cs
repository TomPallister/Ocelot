namespace Ocelot.Headers
{
    using Infrastructure;
    using Logging;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Creator;
    using Ocelot.Infrastructure.Claims.Parser;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Linq;

    public class AddHeadersToRequest : IAddHeadersToRequest
    {
        private readonly IClaimsParser _claimsParser;
        private readonly IPlaceholders _placeholders;
        private readonly IOcelotLogger _logger;

        public AddHeadersToRequest(IClaimsParser claimsParser, IPlaceholders placeholders, IOcelotLoggerFactory factory)
        {
            _logger = factory.CreateLogger<AddHeadersToRequest>();
            _claimsParser = claimsParser;
            _placeholders = placeholders;
        }

        public Response SetHeadersOnDownstreamRequest(List<ClaimToThing> claimsToThings, IEnumerable<System.Security.Claims.Claim> claims, DownstreamRequest downstreamRequest)
        {
            foreach (var config in claimsToThings)
            {
                var value = _claimsParser.GetValue(claims, config.NewKey, config.Delimiter, config.Index);

                if (value.IsError)
                {
                    return new ErrorResponse(value.Errors);
                }

                var exists = downstreamRequest.Headers.FirstOrDefault(x => x.Key == config.ExistingKey);

                if (!string.IsNullOrEmpty(exists.Key))
                {
                    downstreamRequest.Headers.Remove(exists.Key);
                }

                downstreamRequest.Headers.Add(config.ExistingKey, value.Data);
            }

            return new OkResponse();
        }

        public void SetHeadersOnDownstreamRequest(IEnumerable<AddHeader> headers, HttpContext context)
        {
            var requestHeader = context.Request.Headers;

            foreach (var header in headers)
            {
                if (requestHeader.ContainsKey(header.Key))
                {
                    requestHeader.Remove(header.Key);
                }

                if (header.Value.StartsWith("{") && header.Value.EndsWith("}"))
                {
                    var value = _placeholders.Get(header.Value);

                    if (value.IsError)
                    {
                        _logger.LogWarning($"Unable to add header to response {header.Key}: {header.Value}");
                        continue;
                    }

                    requestHeader.Add(header.Key, new StringValues(value.Data));
                }
                else
                {
                    requestHeader.Add(header.Key, header.Value);
                }
            }
        }
    }
}
