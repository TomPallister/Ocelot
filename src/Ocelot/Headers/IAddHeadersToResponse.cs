using Ocelot.Middleware;

namespace Ocelot.Headers
{
    using Ocelot.Configuration.Creator;
    using System.Collections.Generic;

    public interface IAddHeadersToResponse
    {
        void Add(List<AddHeader> addHeaders, DownstreamResponse response);
    }
}
