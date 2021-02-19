namespace Ocelot.Authorization
{
    using Ocelot.Errors;

    public class UnauthorizedError : Error
    {
        public UnauthorizedError(string message)
            : base(message, OcelotErrorCode.UnauthorizedError, 403)
        {
        }
    }
}
