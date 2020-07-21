using Ocelot.Errors;

namespace Ocelot.Infrastructure
{
    public class CouldNotFindPlaceholderError : Error
    {
        public CouldNotFindPlaceholderError(string placeholder)
            : base($"Unable to find placeholder called {placeholder}", OcelotErrorCode.CouldNotFindPlaceholderError, 404)
        {
        }
    }
}
