namespace Ocelot.ServiceDiscovery.Configuration
{
    public class ServiceFabricConfiguration
    {
        public ServiceFabricConfiguration(string hostName, int port, string serviceName)
        {
            HostName = hostName;
            Port = port;
            ServiceName = serviceName;
        }

        public string ServiceName { get; }

        public string HostName { get; }

        public int Port { get; }
    }
}
