using Ocelot.Configuration.Builder;
using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class ServiceProviderConfigurationCreator : IServiceProviderConfigurationCreator
    {
        public ServiceProviderConfiguration Create(FileGlobalConfiguration globalConfiguration)
        {
            var port = globalConfiguration?.ServiceDiscoveryProvider?.Port ?? 0;
            var scheme = globalConfiguration?.ServiceDiscoveryProvider?.Scheme ?? "http";
            var host = globalConfiguration?.ServiceDiscoveryProvider?.Host ?? "localhost";
            var type = !string.IsNullOrEmpty(globalConfiguration?.ServiceDiscoveryProvider?.Type)
                ? globalConfiguration?.ServiceDiscoveryProvider?.Type
                : "consul";
            var pollingInterval = globalConfiguration?.ServiceDiscoveryProvider?.PollingInterval ?? 0;
            var k8snamespace = globalConfiguration?.ServiceDiscoveryProvider?.Namespace ?? string.Empty;

            return new ServiceProviderConfigurationBuilder()
                .WithScheme(scheme)
                .WithHost(host)
                .WithPort(port)
                .WithType(type)
                .WithToken(globalConfiguration?.ServiceDiscoveryProvider?.Token)
                .WithConfigurationKey(globalConfiguration?.ServiceDiscoveryProvider?.ConfigurationKey)
                .WithPollingInterval(pollingInterval)
                .WithNamespace(k8snamespace)
                .Build();
        }
    }
}
