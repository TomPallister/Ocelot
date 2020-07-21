using Ocelot.ServiceDiscovery.Configuration;
using Ocelot.Values;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ocelot.ServiceDiscovery.Providers
{
    public class ServiceFabricServiceDiscoveryProvider : IServiceDiscoveryProvider
    {
        private readonly ServiceFabricConfiguration _configuration;

        public ServiceFabricServiceDiscoveryProvider(ServiceFabricConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task<List<Service>> Get()
        {
            return Task.FromResult(new List<Service>
            {
                new Service(_configuration.ServiceName,
                    new ServiceHostAndPort(_configuration.HostName, _configuration.Port),
                    "doesnt matter with service fabric",
                    "doesnt matter with service fabric",
                    new List<string>())
            });
        }
    }
}
