namespace Ocelot.Provider.Consul
{
    using global::Consul;
    using Infrastructure.Extensions;
    using Logging;
    using ServiceDiscovery.Providers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Values;

    public class Consul : IServiceDiscoveryProvider
    {
        private readonly ConsulRegistryConfiguration _config;
        private readonly IOcelotLogger _logger;
        private readonly IConsulClient _consul;
        private const string VersionPrefix = "version-";

        public Consul(ConsulRegistryConfiguration config, IOcelotLoggerFactory factory, IConsulClientFactory clientFactory)
        {
            _logger = factory.CreateLogger<Consul>();
            _config = config;
            _consul = clientFactory.Get(_config);
        }

        public async Task<List<Service>> Get()
        {
            var queryResult = await _consul.Health.Service(_config.KeyOfServiceInConsul, string.Empty, true);

            var services = new List<Service>();

            foreach (var serviceEntry in queryResult.Response)
            {
                if (IsValid(serviceEntry))
                {
                    var nodes = await _consul.Catalog.Nodes();
                    if (nodes.Response == null)
                    {
                        services.Add(BuildService(serviceEntry, null));
                    }
                    else
                    {
                        var serviceNode = nodes.Response.FirstOrDefault(n => n.Address == serviceEntry.Service.Address);
                        services.Add(BuildService(serviceEntry, serviceNode));
                    }
                }
                else
                {
                    _logger.LogWarning($"Unable to use service Address: {serviceEntry.Service.Address} and Port: {serviceEntry.Service.Port} as it is invalid. Address must contain host only e.g. localhost and port must be greater than 0");
                }
            }

            return services.ToList();
        }

        private Service BuildService(ServiceEntry serviceEntry, Node serviceNode)
        {
            return new Service(
                serviceEntry.Service.Service,
                new ServiceHostAndPort(serviceNode == null ? serviceEntry.Service.Address : serviceNode.Name, serviceEntry.Service.Port),
                serviceEntry.Service.ID,
                GetVersionFromStrings(serviceEntry.Service.Tags),
                serviceEntry.Service.Tags ?? Enumerable.Empty<string>());
        }

        private bool IsValid(ServiceEntry serviceEntry)
        {
            if (string.IsNullOrEmpty(serviceEntry.Service.Address) || serviceEntry.Service.Address.Contains("http://") || serviceEntry.Service.Address.Contains("https://") || serviceEntry.Service.Port <= 0)
            {
                return false;
            }

            return true;
        }

        private string GetVersionFromStrings(IEnumerable<string> strings)
        {
            return strings
                ?.FirstOrDefault(x => x.StartsWith(VersionPrefix, StringComparison.Ordinal))
                .TrimStart(VersionPrefix);
        }
    }
}
