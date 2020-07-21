using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.Headers
{
    public class HttpContextRequestHeaderReplacer : IHttpContextRequestHeaderReplacer
    {
        public Response Replace(HttpContext context, List<HeaderFindAndReplace> fAndRs)
        {
            foreach (var f in fAndRs)
            {
                if (context.Request.Headers.TryGetValue(f.Key, out var values))
                {
                    var replaced = values[f.Index].Replace(f.Find, f.Replace);
                    context.Request.Headers.Remove(f.Key);
                    context.Request.Headers.Add(f.Key, replaced);
                }
            }

            return new OkResponse();
        }
    }
}
