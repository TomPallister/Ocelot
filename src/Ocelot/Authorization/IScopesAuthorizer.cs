using Ocelot.Responses;
using System.Security.Claims;

namespace Ocelot.Authorization
{
    using System.Collections.Generic;

    public interface IScopesAuthorizer
    {
        Response<bool> Authorize(ClaimsPrincipal claimsPrincipal, List<string> routeAllowedScopes, string scopeKey);
    }
}
