using Ocelot.Responses;
using System.Security.Claims;

namespace Ocelot.Authorisation
{
    using System.Collections.Generic;

    public interface IScopesAuthoriser
    {
        Response<bool> Authorise(ClaimsPrincipal claimsPrincipal, List<string> routeAllowedScopes);
    }
}
