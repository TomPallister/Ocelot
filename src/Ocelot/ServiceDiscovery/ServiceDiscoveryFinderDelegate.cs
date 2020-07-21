namespace Ocelot.ServiceDiscovery
{
    using Ocelot.Configuration;
    using Providers;
    using System;

    public delegate IServiceDiscoveryProvider ServiceDiscoveryFinderDelegate(IServiceProvider provider, ServiceProviderConfiguration config, DownstreamRoute route);
}
