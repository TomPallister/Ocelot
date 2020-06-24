namespace Ocelot.LoadBalancer.LoadBalancers
{
    using System.Collections.Generic;
    using System.Linq;
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using Ocelot.ServiceDiscovery;

    public class LoadBalancerFactory : ILoadBalancerFactory
    {
        private readonly IServiceDiscoveryProviderFactory _serviceProviderFactory;
        private readonly IEnumerable<ILoadBalancerCreator> _loadBalancerCreators;

        public LoadBalancerFactory(IServiceDiscoveryProviderFactory serviceProviderFactory, IEnumerable<ILoadBalancerCreator> loadBalancerCreators)
        {
            _serviceProviderFactory = serviceProviderFactory;
            _loadBalancerCreators = loadBalancerCreators;
        }

        public Response<ILoadBalancer> Get(DownstreamRoute route, ServiceProviderConfiguration config)
        {
            var serviceProviderFactoryResponse = _serviceProviderFactory.Get(config, route);

            if (serviceProviderFactoryResponse.IsError)
            {
                return new ErrorResponse<ILoadBalancer>(serviceProviderFactoryResponse.Errors);
            }

            var serviceProvider = serviceProviderFactoryResponse.Data;
            var requestedType = route.LoadBalancerOptions?.Type ?? nameof(NoLoadBalancer);
            var applicableCreator = _loadBalancerCreators.SingleOrDefault(c => c.Type == requestedType);

            if (applicableCreator == null)
            {
                return new ErrorResponse<ILoadBalancer>(new CouldNotFindLoadBalancerCreator($"Could not find load balancer creator for Type: {requestedType}, please check your config specified the correct load balancer and that you have registered a class with the same name."));
            }

            var createdLoadBalancerResponse = applicableCreator.Create(route, serviceProvider);

            if (createdLoadBalancerResponse.IsError)
            {
                return new ErrorResponse<ILoadBalancer>(createdLoadBalancerResponse.Errors);
            }

            return new OkResponse<ILoadBalancer>(createdLoadBalancerResponse.Data);
        }
    }
}
