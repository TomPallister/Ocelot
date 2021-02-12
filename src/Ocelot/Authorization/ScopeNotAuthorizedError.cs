namespace Ocelot.Authorization
{
    using Ocelot.Errors;

    public class ScopeNotAuthorizedError : Error
    {
        public ScopeNotAuthorizedError(string message)
            : base(message, OcelotErrorCode.ScopeNotAuthorizedError, 403)
        {
        }
    }
}
