namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Ocelot.Responses;
    using Ocelot.Configuration;
    using Ocelot.ServiceDiscovery.Providers;

    public interface ILoadBalancerCreator
    {
        Response<ILoadBalancer> Create(DownstreamRoute route, IServiceDiscoveryProvider serviceProvider);
        string Type { get; }
    }
}
