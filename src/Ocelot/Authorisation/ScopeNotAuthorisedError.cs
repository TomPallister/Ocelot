namespace Ocelot.Authorisation
{
    using Ocelot.Errors;

    public class ScopeNotAuthorisedError : Error
    {
        public ScopeNotAuthorisedError(string message)
            : base(message, OcelotErrorCode.ScopeNotAuthorisedError, 403)
        {
        }
    }
}
