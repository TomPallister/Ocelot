namespace Ocelot.Provider.Consul
{
    using global::Consul;

    public interface IConsulClientFactory
    {
        IConsulClient Get(ConsulRegistryConfiguration config);
    }
}
