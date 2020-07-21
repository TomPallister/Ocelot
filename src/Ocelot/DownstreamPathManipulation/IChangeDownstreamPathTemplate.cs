using Ocelot.Configuration;
using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Request.Middleware;
using Ocelot.Responses;
using Ocelot.Values;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Ocelot.PathManipulation
{
    public interface IChangeDownstreamPathTemplate
    {
        Response ChangeDownstreamPath(List<ClaimToThing> claimsToThings, IEnumerable<Claim> claims,
            DownstreamPathTemplate downstreamPathTemplate, List<PlaceholderNameAndValue> placeholders);
    }
}
