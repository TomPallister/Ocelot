using Ocelot.Request.Middleware;
using Ocelot.Responses;
using System;

namespace Ocelot.Infrastructure
{
    public interface IPlaceholders
    {
        Response<string> Get(string key);

        Response<string> Get(string key, DownstreamRequest request);

        Response Add(string key, Func<Response<string>> func);

        Response Remove(string key);
    }
}
