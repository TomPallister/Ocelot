namespace Ocelot.Headers
{
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using System.Collections.Generic;
    using Microsoft.AspNetCore.Http;

    public interface IHttpResponseHeaderReplacer
    {
        public Response Replace(HttpContext httpContext, List<HeaderFindAndReplace> fAndRs);
    }
}
