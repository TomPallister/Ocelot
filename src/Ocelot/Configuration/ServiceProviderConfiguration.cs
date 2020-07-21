namespace Ocelot.Configuration
{
    public class ServiceProviderConfiguration
    {
        public ServiceProviderConfiguration(string type, string scheme, string host, int port, string token, string configurationKey, int pollingInterval, string @namespace = "")
        {
            ConfigurationKey = configurationKey;
            Scheme = scheme;
            Host = host;
            Port = port;
            Token = token;
            Type = type;
            PollingInterval = pollingInterval;
            Namespace = @namespace;
        }

        public string Scheme { get; }

        public string Host { get; }

        public int Port { get; }

        public string Type { get; }

        public string Token { get; }

        public string ConfigurationKey { get; }

        public int PollingInterval { get; }

        public string Namespace { get; }
    }
}
