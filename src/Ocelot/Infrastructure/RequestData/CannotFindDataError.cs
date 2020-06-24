using Ocelot.Errors;

namespace Ocelot.Infrastructure.RequestData
{
    public class CannotFindDataError : Error
    {
        public CannotFindDataError(string message) : base(message, OcelotErrorCode.CannotFindDataError, 404)
        {
        }
    }
}
