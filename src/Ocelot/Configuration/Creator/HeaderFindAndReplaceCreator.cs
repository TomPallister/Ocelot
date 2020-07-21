using Ocelot.Configuration.File;
using Ocelot.Infrastructure;
using Ocelot.Infrastructure.Extensions;
using Ocelot.Logging;
using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.Configuration.Creator
{
    public class HeaderFindAndReplaceCreator : IHeaderFindAndReplaceCreator
    {
        private readonly IPlaceholders _placeholders;
        private readonly IOcelotLogger _logger;

        public HeaderFindAndReplaceCreator(IPlaceholders placeholders, IOcelotLoggerFactory factory)
        {
            _logger = factory.CreateLogger<HeaderFindAndReplaceCreator>();
            _placeholders = placeholders;
        }

        public HeaderTransformations Create(FileRoute fileRoute)
        {
            var upstream = new List<HeaderFindAndReplace>();
            var addHeadersToUpstream = new List<AddHeader>();

            foreach (var input in fileRoute.UpstreamHeaderTransform)
            {
                if (input.Value.Contains(","))
                {
                    var hAndr = Map(input);
                    if (!hAndr.IsError)
                    {
                        upstream.Add(hAndr.Data);
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to add UpstreamHeaderTransform {input.Key}: {input.Value}");
                    }
                }
                else
                {
                    addHeadersToUpstream.Add(new AddHeader(input.Key, input.Value));
                }
            }

            var downstream = new List<HeaderFindAndReplace>();
            var addHeadersToDownstream = new List<AddHeader>();

            foreach (var input in fileRoute.DownstreamHeaderTransform)
            {
                if (input.Value.Contains(","))
                {
                    var hAndr = Map(input);
                    if (!hAndr.IsError)
                    {
                        downstream.Add(hAndr.Data);
                    }
                    else
                    {
                        _logger.LogWarning($"Unable to add DownstreamHeaderTransform {input.Key}: {input.Value}");
                    }
                }
                else
                {
                    addHeadersToDownstream.Add(new AddHeader(input.Key, input.Value));
                }
            }

            return new HeaderTransformations(upstream, downstream, addHeadersToDownstream, addHeadersToUpstream);
        }

        private Response<HeaderFindAndReplace> Map(KeyValuePair<string, string> input)
        {
            var findAndReplace = input.Value.Split(",");

            var replace = findAndReplace[1].TrimStart();

            var startOfPlaceholder = replace.IndexOf("{");
            if (startOfPlaceholder > -1)
            {
                var endOfPlaceholder = replace.IndexOf("}", startOfPlaceholder);

                var placeholder = replace.Substring(startOfPlaceholder, startOfPlaceholder + (endOfPlaceholder + 1));

                var value = _placeholders.Get(placeholder);

                if (value.IsError)
                {
                    return new ErrorResponse<HeaderFindAndReplace>(value.Errors);
                }

                replace = replace.Replace(placeholder, value.Data);
            }

            var hAndr = new HeaderFindAndReplace(input.Key, findAndReplace[0], replace, 0);

            return new OkResponse<HeaderFindAndReplace>(hAndr);
        }
    }
}
