namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Errors;

    public class CouldNotFindLoadBalancerCreator : Error
    {
        public CouldNotFindLoadBalancerCreator(string message) 
            : base(message, OcelotErrorCode.CouldNotFindLoadBalancerCreator, 404)
        {
        }
    }
}
