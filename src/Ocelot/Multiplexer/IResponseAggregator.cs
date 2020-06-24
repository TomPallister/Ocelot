namespace Ocelot.Multiplexer
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IResponseAggregator
    {
        Task Aggregate(Route route, HttpContext originalContext, List<HttpContext> downstreamResponses);
    }
}
