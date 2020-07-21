using Ocelot.Middleware;
using Ocelot.Responses;
using System.Collections.Generic;

namespace Ocelot.Headers
{
    public interface IRemoveOutputHeaders
    {
        Response Remove(List<Header> headers);
    }
}
