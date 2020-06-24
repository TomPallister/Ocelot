using Ocelot.Errors;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class UnableToFindLoadBalancerError : Errors.Error
    {
        public UnableToFindLoadBalancerError(string message)
            : base(message, OcelotErrorCode.UnableToFindLoadBalancerError, 404)
        {
        }
    }
}
