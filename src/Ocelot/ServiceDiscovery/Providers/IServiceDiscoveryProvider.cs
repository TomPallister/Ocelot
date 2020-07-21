using Ocelot.Values;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ocelot.ServiceDiscovery.Providers
{
    public interface IServiceDiscoveryProvider
    {
        Task<List<Service>> Get();
    }
}
