using Ocelot.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Ocelot.Authorization
{
    using Infrastructure.Claims.Parser;

    public class ScopesAuthorizer : IScopesAuthorizer
    {
        private readonly IClaimsParser _claimsParser;

        public ScopesAuthorizer(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response<bool> Authorize(ClaimsPrincipal claimsPrincipal, List<string> routeAllowedScopes, string scopeKey)
        {
            if (routeAllowedScopes == null || routeAllowedScopes.Count == 0)
            {
                return new OkResponse<bool>(true);
            }

            scopeKey ??= "scope";
            var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, scopeKey);

            if (values.IsError)
            {
                return new ErrorResponse<bool>(values.Errors);
            }

            var userScopes = values.Data;

            var matchesScopes = routeAllowedScopes.Intersect(userScopes).ToList();

            if (matchesScopes.Count == 0)
            {
                return new ErrorResponse<bool>(
                    new ScopeNotAuthorizedError($"no one user scope: '{string.Join(",", userScopes)}' match with some allowed scope: '{string.Join(",", routeAllowedScopes)}'"));
            }

            return new OkResponse<bool>(true);
        }
    }
}
