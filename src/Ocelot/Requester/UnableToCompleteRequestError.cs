using Ocelot.Errors;
using System;

namespace Ocelot.Requester
{
    public class UnableToCompleteRequestError : Error
    {
        public UnableToCompleteRequestError(Exception exception)
            : base($"Error making http request, exception: {exception}", OcelotErrorCode.UnableToCompleteRequestError, 500)
        {
        }
    }
}
