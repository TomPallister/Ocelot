namespace Ocelot.Multiplexer
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Middleware;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class UserDefinedResponseAggregator : IResponseAggregator
    {
        private readonly IDefinedAggregatorProvider _provider;

        public UserDefinedResponseAggregator(IDefinedAggregatorProvider provider)
        {
            _provider = provider;
        }

        public async Task Aggregate(Route route, HttpContext originalContext, List<HttpContext> downstreamResponses)
        {
            var aggregator = _provider.Get(route);

            if (!aggregator.IsError)
            {
                var aggregateResponse = await aggregator.Data
                    .Aggregate(downstreamResponses);

                originalContext.Items.UpsertDownstreamResponse(aggregateResponse);
            }
            else
            {
                originalContext.Items.UpsertErrors(aggregator.Errors);
            }
        }
    }
}
