namespace Ocelot.ServiceDiscovery
{
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.Configuration;
    using Ocelot.Logging;
    using Ocelot.Responses;
    using Ocelot.ServiceDiscovery.Configuration;
    using Ocelot.ServiceDiscovery.Providers;
    using Ocelot.Values;
    using System;
    using System.Collections.Generic;

    public class ServiceDiscoveryProviderFactory : IServiceDiscoveryProviderFactory
    {
        private readonly IOcelotLoggerFactory _factory;
        private readonly ServiceDiscoveryFinderDelegate _delegates;
        private readonly IServiceProvider _provider;

        public ServiceDiscoveryProviderFactory(IOcelotLoggerFactory factory, IServiceProvider provider)
        {
            _factory = factory;
            _provider = provider;
            _delegates = provider.GetService<ServiceDiscoveryFinderDelegate>();
        }

        public Response<IServiceDiscoveryProvider> Get(ServiceProviderConfiguration serviceConfig, DownstreamRoute route)
        {
            if (route.UseServiceDiscovery)
            {
                return GetServiceDiscoveryProvider(serviceConfig, route);
            }

            var services = new List<Service>();

            foreach (var downstreamAddress in route.DownstreamAddresses)
            {
                var service = new Service(route.ServiceName, new ServiceHostAndPort(downstreamAddress.Host, downstreamAddress.Port, route.DownstreamScheme), string.Empty, string.Empty, new string[0]);

                services.Add(service);
            }

            return new OkResponse<IServiceDiscoveryProvider>(new ConfigurationServiceProvider(services));
        }

        private Response<IServiceDiscoveryProvider> GetServiceDiscoveryProvider(ServiceProviderConfiguration config, DownstreamRoute route)
        {
            if (config.Type?.ToLower() == "servicefabric")
            {
                var sfConfig = new ServiceFabricConfiguration(config.Host, config.Port, route.ServiceName);
                return new OkResponse<IServiceDiscoveryProvider>(new ServiceFabricServiceDiscoveryProvider(sfConfig));
            }

            if (_delegates != null)
            {
                var provider = _delegates?.Invoke(_provider, config, route);

                if (provider.GetType().Name.ToLower() == config.Type.ToLower())
                {
                    return new OkResponse<IServiceDiscoveryProvider>(provider);
                }
            }

            return new ErrorResponse<IServiceDiscoveryProvider>(new UnableToFindServiceDiscoveryProviderError($"Unable to find service discovery provider for type: {config.Type}"));
        }
    }
}
