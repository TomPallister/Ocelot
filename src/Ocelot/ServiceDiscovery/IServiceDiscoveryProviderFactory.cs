namespace Ocelot.ServiceDiscovery
{
    using Ocelot.Configuration;
    using Ocelot.Responses;
    using Ocelot.ServiceDiscovery.Providers;

    public interface IServiceDiscoveryProviderFactory
    {
        Response<IServiceDiscoveryProvider> Get(ServiceProviderConfiguration serviceConfig, DownstreamRoute route);
    }
}
