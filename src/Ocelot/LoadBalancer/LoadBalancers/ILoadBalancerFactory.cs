namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Ocelot.Configuration;
    using Ocelot.Responses;

    public interface ILoadBalancerFactory
    {
        Response<ILoadBalancer> Get(DownstreamRoute route, ServiceProviderConfiguration config);
    }
}
