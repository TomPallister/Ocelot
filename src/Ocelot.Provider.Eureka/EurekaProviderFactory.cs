namespace Ocelot.Provider.Eureka
{
    using Microsoft.Extensions.DependencyInjection;
    using Ocelot.ServiceDiscovery;
    using Steeltoe.Discovery;

    public static class EurekaProviderFactory
    {
        public static ServiceDiscoveryFinderDelegate Get = (provider, config, route) =>
        {
            var client = provider.GetService<IDiscoveryClient>();

            if (config.Type?.ToLower() == "eureka" && client != null)
            {
                return new Eureka(route.ServiceName, client);
            }

            return null;
        };
    }
}
