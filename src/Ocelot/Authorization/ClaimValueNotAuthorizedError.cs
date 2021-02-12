namespace Ocelot.Authorization
{
    using Ocelot.Errors;
    using System.Net;

    public class ClaimValueNotAuthorizedError : Error
    {
        public ClaimValueNotAuthorizedError(string message)
            : base(message, OcelotErrorCode.ClaimValueNotAuthorizedError, 403)
        {
        }
    }
}
