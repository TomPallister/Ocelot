using KubeClient;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.Logging;
using Ocelot.ServiceDiscovery;
using System;
using Ocelot.Configuration;

namespace Ocelot.Provider.Kubernetes
{
    public static class KubernetesProviderFactory
    {
        public static ServiceDiscoveryFinderDelegate Get = (provider, config, route) =>
        {
            var factory = provider.GetService<IOcelotLoggerFactory>();
            return GetKubeProvider(provider, config, route, factory);
        };

        private static ServiceDiscovery.Providers.IServiceDiscoveryProvider GetKubeProvider(IServiceProvider provider, ServiceProviderConfiguration config, DownstreamRoute route, IOcelotLoggerFactory factory)
        {
            var kubeClient = provider.GetService<IKubeApiClient>();

            var k8sRegistryConfiguration = new KubeRegistryConfiguration()
            {
                KeyOfServiceInK8s = route.ServiceName,
                KubeNamespace = string.IsNullOrEmpty(route.ServiceNamespace) ? config.Namespace : route.ServiceNamespace
            };

            var k8sServiceDiscoveryProvider = new KubernetesServiceDiscoveryProvider(k8sRegistryConfiguration, factory, kubeClient);

            if (config.Type?.ToLower() == "pollkube")
            {
                return new PollKubernetes(config.PollingInterval, factory, k8sServiceDiscoveryProvider);
            }
            return k8sServiceDiscoveryProvider;
        }
    }
}
