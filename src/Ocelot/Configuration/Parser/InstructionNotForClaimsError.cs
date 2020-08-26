using Ocelot.Errors;

namespace Ocelot.Configuration.Parser
{
    public class InstructionNotForClaimsError : Error
    {
        public InstructionNotForClaimsError()
            : base("instructions did not contain claims, at the moment we only support claims extraction", OcelotErrorCode.InstructionNotForClaimsError, 404)
        {
        }
    }
}
