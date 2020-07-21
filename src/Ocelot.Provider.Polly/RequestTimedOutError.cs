namespace Ocelot.Provider.Polly
{
    using Ocelot.Errors;
    using System;

    public class RequestTimedOutError : Error
    {
        public RequestTimedOutError(Exception exception)
            : base($"Timeout making http request, exception: {exception}", OcelotErrorCode.RequestTimedOutError, 503)
        {
        }
    }
}
