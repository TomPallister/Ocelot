using System.Collections.Generic;
using System.Security.Claims;
using Ocelot.Responses;

namespace Ocelot.Authorization
{
    public interface IRolesAuthorizer
    {
        Response<bool> Authorize(ClaimsPrincipal claimsPrincipal, List<string> routeRequiredRole, string roleKey);
    }
}