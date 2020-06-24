using Ocelot.Errors;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class ServicesAreNullError : Error
    {
        public ServicesAreNullError(string message)
            : base(message, OcelotErrorCode.ServicesAreNullError, 404)
        {
        }
    }
}
