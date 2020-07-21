using Ocelot.DownstreamRouteFinder.UrlMatcher;
using Ocelot.Responses;
using System.Security.Claims;

namespace Ocelot.Authorisation
{
    using System.Collections.Generic;

    public interface IClaimsAuthoriser
    {
        Response<bool> Authorise(
            ClaimsPrincipal claimsPrincipal,
            Dictionary<string, string> routeClaimsRequirement,
            List<PlaceholderNameAndValue> urlPathPlaceholderNameAndValues
        );
    }
}
