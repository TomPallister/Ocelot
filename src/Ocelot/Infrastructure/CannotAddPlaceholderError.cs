using Ocelot.Errors;

namespace Ocelot.Infrastructure
{
    public class CannotAddPlaceholderError : Error
    {
        public CannotAddPlaceholderError(string message)
            : base(message, OcelotErrorCode.CannotAddPlaceholderError, 404)
        {
        }
    }
}
