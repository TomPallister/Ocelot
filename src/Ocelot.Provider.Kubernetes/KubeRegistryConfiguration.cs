namespace Ocelot.Provider.Kubernetes
{
    public class KubeRegistryConfiguration
    {
        public string KubeNamespace { get; set; }

        public string KeyOfServiceInK8s { get; set; }
    }
}
