namespace Ocelot.Infrastructure.Claims.Parser
{
    using Ocelot.Errors;

    public class CannotFindClaimError : Error
    {
        public CannotFindClaimError(string message)
            : base(message, OcelotErrorCode.CannotFindClaimError, 403)
        {
        }
    }
}
