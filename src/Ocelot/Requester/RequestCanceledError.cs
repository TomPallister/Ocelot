using Ocelot.Errors;

namespace Ocelot.Requester
{
    public class RequestCanceledError : Error
    {
        public RequestCanceledError(string message)
            // status code refer to
            // https://stackoverflow.com/questions/46234679/what-is-the-correct-http-status-code-for-a-cancelled-request?answertab=votes#tab-top
            // https://httpstatuses.com/499
            : base(message, OcelotErrorCode.RequestCanceled, 499)
        {
        }
    }
}
