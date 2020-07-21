namespace Ocelot.Values
{
    public class ServiceHostAndPort
    {
        public ServiceHostAndPort(string downstreamHost, int downstreamPort)
        {
            DownstreamHost = downstreamHost?.Trim('/');
            DownstreamPort = downstreamPort;
        }

        public ServiceHostAndPort(string downstreamHost, int downstreamPort, string scheme)
            : this(downstreamHost, downstreamPort) => Scheme = scheme;

        public string DownstreamHost { get; }

        public int DownstreamPort { get; }

        public string Scheme { get; }
    }
}
