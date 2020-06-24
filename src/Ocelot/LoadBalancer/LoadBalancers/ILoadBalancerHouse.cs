using Ocelot.Configuration;
using Ocelot.Responses;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public interface ILoadBalancerHouse
    {
        Response<ILoadBalancer> Get(DownstreamRoute route, ServiceProviderConfiguration config);
    }
}
