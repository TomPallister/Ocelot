using KubeClient;

namespace Ocelot.Provider.Kubernetes
{
    public class KubeApiClientFactory : IKubeApiClientFactory
    {
        public IKubeApiClient Get(KubeRegistryConfiguration config)
        {
            var option = new KubeClientOptions
            {
                ApiEndPoint = config.ApiEndPoint
            };
            if (!string.IsNullOrEmpty(config?.AccessToken))
            {
                option.AccessToken = config.AccessToken;
                option.AuthStrategy = config.AuthStrategy;
                option.AllowInsecure = config.AllowInsecure;
            }
            return KubeApiClient.Create(option);
        }
    }
}
