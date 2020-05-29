using KubeClient;
using KubeClient.Models;
using Ocelot.Logging;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ocelot.Provider.Kubernetes.KubeApiClientExtensions;

namespace Ocelot.Provider.Kubernetes
{
    public class KubernetesServiceDiscoveryProvider : IServiceDiscoveryProvider
    {
        private readonly KubeRegistryConfiguration _kubeRegistryConfiguration;
        private readonly IOcelotLogger _logger;
        private readonly IKubeApiClient _kubeApi;

        public KubernetesServiceDiscoveryProvider(KubeRegistryConfiguration kubeRegistryConfiguration, IOcelotLoggerFactory factory, IKubeApiClient kubeApi)
        {
            _kubeRegistryConfiguration = kubeRegistryConfiguration;
            _logger = factory.CreateLogger<KubernetesServiceDiscoveryProvider>();
            _kubeApi = kubeApi;
        }

        public async Task<List<Service>> Get()
        {
            var endpoint = await _kubeApi
                .ResourceClient(client => new EndPointClientV1(client))
                .Get(_kubeRegistryConfiguration.KeyOfServiceInK8s, _kubeRegistryConfiguration.KubeNamespace);

            var services = new List<Service>();
            if (endpoint != null && endpoint.Subsets.Any())
            {
                services.AddRange(BuildServices(endpoint));
            }
            else
            {
                _logger.LogWarning($"namespace:{_kubeRegistryConfiguration.KubeNamespace }service:{_kubeRegistryConfiguration.KeyOfServiceInK8s} Unable to use ,it is invalid. Address must contain host only e.g. localhost and port must be greater than 0");
            }
            return services;
        }

        private List<Service> BuildServices(EndpointsV1 endpoint)
        {
            var services = new List<Service>();

            foreach (var subset in endpoint.Subsets)
            {
                services.AddRange(subset.Addresses.Select(address => new Service(endpoint.Metadata.Name,
                    new ServiceHostAndPort(address.Ip, subset.Ports.First().Port),
                    endpoint.Metadata.Uid, string.Empty, Enumerable.Empty<string>())));
            }
            return services;
        }
    }
}
