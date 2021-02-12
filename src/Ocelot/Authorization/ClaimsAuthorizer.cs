namespace Ocelot.Authorization
{
    using Ocelot.Infrastructure.Claims.Parser;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Text.RegularExpressions;

    public class ClaimsAuthorizer : IClaimsAuthorizer
    {
        private readonly IClaimsParser _claimsParser;

        public ClaimsAuthorizer(IClaimsParser claimsParser)
        {
            _claimsParser = claimsParser;
        }

        public Response<bool> Authorize(
            ClaimsPrincipal claimsPrincipal,
            Dictionary<string, string> routeClaimsRequirement,
            List<PlaceholderNameAndValue> urlPathPlaceholderNameAndValues
        )
        {
            foreach (var required in routeClaimsRequirement)
            {
                var values = _claimsParser.GetValuesByClaimType(claimsPrincipal.Claims, required.Key);

                if (values.IsError)
                {
                    return new ErrorResponse<bool>(values.Errors);
                }

                if (values.Data != null)
                {
                    // dynamic claim
                    var match = Regex.Match(required.Value, @"^{(?<variable>.+)}$");
                    if (match.Success)
                    {
                        var variableName = match.Captures[0].Value;

                        var matchingPlaceholders = urlPathPlaceholderNameAndValues.Where(p => p.Name.Equals(variableName)).Take(2).ToArray();
                        if (matchingPlaceholders.Length == 1)
                        {
                            // match
                            var actualValue = matchingPlaceholders[0].Value;
                            var authorized = values.Data.Contains(actualValue);
                            if (!authorized)
                            {
                                return new ErrorResponse<bool>(new ClaimValueNotAuthorizedError(
                                    $"dynamic claim value for {variableName} of {string.Join(", ", values.Data)} is not the same as required value: {actualValue}"));
                            }
                        }
                        else
                        {
                            // config error
                            if (matchingPlaceholders.Length == 0)
                            {
                                return new ErrorResponse<bool>(new ClaimValueNotAuthorizedError(
                                    $"config error: requires variable claim value: {variableName} placeholders does not contain that variable: {string.Join(", ", urlPathPlaceholderNameAndValues.Select(p => p.Name))}"));
                            }
                            else
                            {
                                return new ErrorResponse<bool>(new ClaimValueNotAuthorizedError(
                                    $"config error: requires variable claim value: {required.Value} but placeholders are ambiguous: {string.Join(", ", urlPathPlaceholderNameAndValues.Where(p => p.Name.Equals(variableName)).Select(p => p.Value))}"));
                            }
                        }
                    }
                    else
                    {
                        // static claim
                        var authorized = values.Data.Contains(required.Value);
                        if (!authorized)
                        {
                            return new ErrorResponse<bool>(new ClaimValueNotAuthorizedError(
                                       $"claim value: {string.Join(", ", values.Data)} is not the same as required value: {required.Value} for type: {required.Key}"));
                        }
                    }
                }
                else
                {
                    return new ErrorResponse<bool>(new UserDoesNotHaveClaimError($"user does not have claim {required.Key}"));
                }
            }

            return new OkResponse<bool>(true);
        }
    }
}
