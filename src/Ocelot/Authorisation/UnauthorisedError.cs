namespace Ocelot.Authorisation
{
    using Ocelot.Errors;

    public class UnauthorisedError : Error
    {
        public UnauthorisedError(string message)
            : base(message, OcelotErrorCode.UnauthorizedError, 403)
        {
        }
    }
}
