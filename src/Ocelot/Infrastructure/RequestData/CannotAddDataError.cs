using Ocelot.Errors;

namespace Ocelot.Infrastructure.RequestData
{
    public class CannotAddDataError : Error
    {
        public CannotAddDataError(string message) : base(message, OcelotErrorCode.CannotAddDataError, 404)
        {
        }
    }
}
