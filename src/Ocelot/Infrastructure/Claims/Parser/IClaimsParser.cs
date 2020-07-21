namespace Ocelot.Infrastructure.Claims.Parser
{
    using Responses;
    using System.Collections.Generic;
    using System.Security.Claims;

    public interface IClaimsParser
    {
        Response<string> GetValue(IEnumerable<Claim> claims, string key, string delimiter, int index);

        Response<List<string>> GetValuesByClaimType(IEnumerable<Claim> claims, string claimType);
    }
}
