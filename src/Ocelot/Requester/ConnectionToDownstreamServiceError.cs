using Ocelot.Errors;
using System;

namespace Ocelot.Requester
{
    public class ConnectionToDownstreamServiceError : Error
    {
        public ConnectionToDownstreamServiceError(Exception exception)
            : base($"Error connecting to downstream service, exception: {exception}", OcelotErrorCode.ConnectionToDownstreamServiceError, 502)
        {
        }
    }
}
