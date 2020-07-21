using Ocelot.Configuration.File;
using System.Collections.Generic;
using System.Linq;

namespace Ocelot.Configuration.Creator
{
    public class DownstreamAddressesCreator : IDownstreamAddressesCreator
    {
        public List<DownstreamHostAndPort> Create(FileRoute route)
        {
            return route.DownstreamHostAndPorts.Select(hostAndPort => new DownstreamHostAndPort(hostAndPort.Host, hostAndPort.Port)).ToList();
        }
    }
}
