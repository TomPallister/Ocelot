using Ocelot.Configuration.File;
using System.Collections.Generic;
using System.Linq;
using Ocelot.Logging;

namespace Ocelot.Configuration.Creator
{
    public class DownstreamAddressesCreator : IDownstreamAddressesCreator
    {
        private readonly IOcelotLogger _logger;

        public DownstreamAddressesCreator(IOcelotLoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DownstreamAddressesCreator>();
        }

        public List<DownstreamHostAndPort> Create(FileRoute route, FileGlobalConfiguration globalConfiguration)
        {
            return EnumerateDownstreamHosts(route, globalConfiguration).ToList();
        }

        private IEnumerable<DownstreamHostAndPort> EnumerateDownstreamHosts(FileRoute route, FileGlobalConfiguration fileGlobalConfiguration)
        {
            foreach (var downstreamHost in route.DownstreamHostAndPorts)
            {
                if (string.IsNullOrWhiteSpace(downstreamHost.GlobalHostKey) == false)
                {
                    if (fileGlobalConfiguration.DownstreamHosts.TryGetValue(downstreamHost.GlobalHostKey, out var globalDownstreamHost))
                    {
                        yield return new DownstreamHostAndPort(globalDownstreamHost.Host, globalDownstreamHost.Port);
                    }
                    else
                    {
                        _logger.LogWarning($"Global configuration doesn't contain the definition of '{downstreamHost.GlobalHostKey}' downstream host");
                    }
                }
                else
                {
                    yield return new DownstreamHostAndPort(downstreamHost.Host, downstreamHost.Port);
                }
            }
        }
    }
}
