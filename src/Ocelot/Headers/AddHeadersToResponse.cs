namespace Ocelot.Headers
{
    using Ocelot.Configuration.Creator;
    using Ocelot.Infrastructure;
    using Ocelot.Infrastructure.Extensions;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System.Collections.Generic;

    public class AddHeadersToResponse : IAddHeadersToResponse
    {
        private readonly IPlaceholders _placeholders;
        private readonly IOcelotLogger _logger;

        public AddHeadersToResponse(IPlaceholders placeholders, IOcelotLoggerFactory factory)
        {
            _logger = factory.CreateLogger<AddHeadersToResponse>();
            _placeholders = placeholders;
        }

        public void Add(List<AddHeader> addHeaders, DownstreamResponse response)
        {
            foreach (var add in addHeaders)
            {
                if (add.Value.StartsWith('{') && add.Value.EndsWith('}'))
                {
                    var value = _placeholders.Get(add.Value);

                    if (value.IsError)
                    {
                        _logger.LogWarning($"Unable to add header to response {add.Key}: {add.Value}");
                        continue;
                    }

                    response.Headers.Add(new Header(add.Key, new List<string> { value.Data }));
                }
                else
                {
                    response.Headers.Add(new Header(add.Key, new List<string> { add.Value }));
                }
            }
        }
    }
}
