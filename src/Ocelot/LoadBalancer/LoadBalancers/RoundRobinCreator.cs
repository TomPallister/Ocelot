namespace Ocelot.LoadBalancer.LoadBalancers
{
    using Ocelot.Configuration;
    using Ocelot.ServiceDiscovery.Providers;
    using Ocelot.Responses;

    public class RoundRobinCreator : ILoadBalancerCreator
    {
        public Response<ILoadBalancer> Create(DownstreamRoute route, IServiceDiscoveryProvider serviceProvider)
        {
            return new OkResponse<ILoadBalancer>(new RoundRobin(async () => await serviceProvider.Get()));
        }

        public string Type => nameof(RoundRobin);
    }
}
