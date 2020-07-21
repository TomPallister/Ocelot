using KubeClient;

namespace Ocelot.Provider.Kubernetes
{
    public interface IKubeApiClientFactory
    {
        IKubeApiClient Get(KubeRegistryConfiguration config);
    }
}
