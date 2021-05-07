namespace Ocelot.Requester
{
    using Microsoft.AspNetCore.Http;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;

    public interface IDelegatingHandlerHandlerFactory
    {
        Response<List<Func<DelegatingHandler>>> Get(DownstreamRoute downstreamRoute, HttpContext httpContext);
    }
}
