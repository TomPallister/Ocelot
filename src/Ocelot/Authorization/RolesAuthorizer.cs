using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Ocelot.Infrastructure.Claims.Parser;
using Ocelot.Responses;

namespace Ocelot.Authorization
{
    public class RolesAuthorizer : IRolesAuthorizer
    {
        private readonly IClaimsParser _claimsParser;

        public RolesAuthorizer(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response<bool> Authorize(ClaimsPrincipal claimsPrincipal, List<string> routeRequiredRole, string roleKey)
        {
            if (routeRequiredRole == null || routeRequiredRole.Count == 0)
            {
                return new OkResponse<bool>(true);
            }

            roleKey ??= "role";

            var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, roleKey);

            if (values.IsError)
            {
                return new ErrorResponse<bool>(values.Errors);
            }

            var userRoles = values.Data;

            var matchedRoles = routeRequiredRole.Intersect(userRoles).ToList(); // Note this is an OR

            if (matchedRoles.Count == 0)
            {
                return new ErrorResponse<bool>(
                    new ScopeNotAuthorizedError($"no one user role: '{string.Join(",", userRoles)}' match with some allowed role: '{string.Join(",", routeRequiredRole)}'"));
            }

            return new OkResponse<bool>(true);
        }
    }
}