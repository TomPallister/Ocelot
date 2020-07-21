using Ocelot.Errors;

namespace Ocelot.Infrastructure
{
    public class CannotRemovePlaceholderError : Error
    {
        public CannotRemovePlaceholderError(string message)
            : base(message, OcelotErrorCode.CannotRemovePlaceholderError, 404)
        {
        }
    }
}
