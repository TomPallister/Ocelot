namespace Ocelot.Authorisation
{
    using Ocelot.Errors;
    using System.Net;

    public class ClaimValueNotAuthorisedError : Error
    {
        public ClaimValueNotAuthorisedError(string message)
            : base(message, OcelotErrorCode.ClaimValueNotAuthorisedError, 403)
        {
        }
    }
}
