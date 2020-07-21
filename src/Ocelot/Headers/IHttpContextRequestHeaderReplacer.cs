using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.Headers
{
    public interface IHttpContextRequestHeaderReplacer
    {
        Response Replace(HttpContext context, List<HeaderFindAndReplace> fAndRs);
    }
}
