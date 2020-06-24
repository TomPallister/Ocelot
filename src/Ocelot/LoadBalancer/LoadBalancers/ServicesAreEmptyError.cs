using Ocelot.Errors;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class ServicesAreEmptyError : Error
    {
        public ServicesAreEmptyError(string message)
            : base(message, OcelotErrorCode.ServicesAreEmptyError, 404)
        {
        }
    }
}
