namespace Ocelot.Headers
{
    using Ocelot.Configuration;
    using Ocelot.Infrastructure;
    using Ocelot.Infrastructure.Extensions;
    using Ocelot.Middleware;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class HttpResponseHeaderReplacer : IHttpResponseHeaderReplacer
    {
        private readonly IPlaceholders _placeholders;

        public HttpResponseHeaderReplacer(IPlaceholders placeholders)
        {
            _placeholders = placeholders;
        }

        public Response Replace(HttpContext httpContext, List<HeaderFindAndReplace> fAndRs)
        {
            var response = httpContext.Items.DownstreamResponse();
            var request = httpContext.Items.DownstreamRequest();

            foreach (var f in fAndRs)
            {
                var dict = response.Headers.ToDictionary(x => x.Key);

                //if the response headers contain a matching find and replace
                if (dict.TryGetValue(f.Key, out var values))
                {
                    //check to see if it is a placeholder in the find...
                    var placeholderValue = _placeholders.Get(f.Find, request);

                    if (!placeholderValue.IsError)
                    {
                        //if it is we need to get the value of the placeholder
                        var replaced = values.Values.ToList()[f.Index].Replace(placeholderValue.Data, f.Replace.LastCharAsForwardSlash());

                        response.Headers.Remove(response.Headers.First(item => item.Key == f.Key));
                        response.Headers.Add(new Header(f.Key, new List<string> { replaced }));
                    }
                    else
                    {
                        var replaced = values.Values.ToList()[f.Index].Replace(f.Find, f.Replace);

                        response.Headers.Remove(response.Headers.First(item => item.Key == f.Key));
                        response.Headers.Add(new Header(f.Key, new List<string> { replaced }));
                    }
                }
            }

            return new OkResponse();
        }
    }
}
