using Microsoft.AspNetCore.Http;

namespace Ocelot.Headers
{
    using Ocelot.Configuration;
    using Ocelot.Configuration.Creator;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using System.Collections.Generic;

    public interface IAddHeadersToRequest
    {
        Response SetHeadersOnDownstreamRequest(List<ClaimToThing> claimsToThings, IEnumerable<System.Security.Claims.Claim> claims, DownstreamRequest downstreamRequest);

        void SetHeadersOnDownstreamRequest(IEnumerable<AddHeader> headers, HttpContext context);
    }
}
