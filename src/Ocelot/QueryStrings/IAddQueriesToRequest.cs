using Ocelot.Configuration;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using System.Collections.Generic;
using System.Security.Claims;

namespace Ocelot.QueryStrings
{
    public interface IAddQueriesToRequest
    {
        Response SetQueriesOnDownstreamRequest(List<ClaimToThing> claimsToThings, IEnumerable<Claim> claims, DownstreamRequest downstreamRequest);
    }
}
