using System.Net;

namespace Ocelot.Errors
{
    public abstract class Error
    {
        protected Error(string message, OcelotErrorCode code, int httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
            Message = message;
            Code = code;
        }

        public string Message { get; private set; }
        public OcelotErrorCode Code { get; private set; }
        public int HttpStatusCode { get; private set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
