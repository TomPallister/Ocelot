using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.Claims
{
    public interface IAddClaimsToRequest
    {
        Response SetClaimsOnContext(List<ClaimToThing> claimsToThings,
            HttpContext context);
    }
}
