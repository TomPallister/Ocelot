using Ocelot.Middleware;
using Ocelot.Responses;
using System.Collections.Generic;
using System.Linq;

namespace Ocelot.Headers
{
    public class RemoveOutputHeaders : IRemoveOutputHeaders
    {
        /// <summary>
        /// Some webservers return headers that cannot be forwarded to the client
        /// in a given context such as transfer encoding chunked when ASP.NET is not
        /// returning the response in this manner
        /// </summary>
        private readonly string[] _unsupportedRequestHeaders =
        {
            "Transfer-Encoding"
        };

        public Response Remove(List<Header> headers)
        {
            headers.RemoveAll(x => _unsupportedRequestHeaders.Contains(x.Key));
            return new OkResponse();
        }
    }
}
