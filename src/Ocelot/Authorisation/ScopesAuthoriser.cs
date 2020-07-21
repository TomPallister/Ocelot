using Ocelot.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Ocelot.Authorisation
{
    using Infrastructure.Claims.Parser;

    public class ScopesAuthoriser : IScopesAuthoriser
    {
        private readonly IClaimsParser _claimsParser;
        private readonly string _scope = "scope";

        public ScopesAuthoriser(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response<bool> Authorise(ClaimsPrincipal claimsPrincipal, List<string> routeAllowedScopes)
        {
            if (routeAllowedScopes == null || routeAllowedScopes.Count == 0)
            {
                return new OkResponse<bool>(true);
            }

            var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, _scope);

            if (values.IsError)
            {
                return new ErrorResponse<bool>(values.Errors);
            }

            var userScopes = values.Data;

            var matchesScopes = routeAllowedScopes.Intersect(userScopes).ToList();

            if (matchesScopes.Count == 0)
            {
                return new ErrorResponse<bool>(
                    new ScopeNotAuthorisedError($"no one user scope: '{string.Join(",", userScopes)}' match with some allowed scope: '{string.Join(",", routeAllowedScopes)}'"));
            }

            return new OkResponse<bool>(true);
        }
    }
}
