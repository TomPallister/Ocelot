namespace Ocelot.Provider.Consul
{
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceDiscovery;

    public static class ConsulProviderFactory
    {
        public static ServiceDiscoveryFinderDelegate Get = (provider, config, route) =>
        {
            var factory = provider.GetService<IOcelotLoggerFactory>();

            var consulFactory = provider.GetService<IConsulClientFactory>();

            var consulRegistryConfiguration = new ConsulRegistryConfiguration(config.Scheme, config.Host, config.Port, route.ServiceName, config.Token);

            var consulServiceDiscoveryProvider = new Consul(consulRegistryConfiguration, factory, consulFactory);

            if (config.Type?.ToLower() == "pollconsul")
            {
                return new PollConsul(config.PollingInterval, factory, consulServiceDiscoveryProvider);
            }

            return consulServiceDiscoveryProvider;
        };
    }
}
