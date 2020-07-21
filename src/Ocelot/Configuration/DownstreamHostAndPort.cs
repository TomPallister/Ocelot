namespace Ocelot.Configuration
{
    public class DownstreamHostAndPort
    {
        public DownstreamHostAndPort(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public string Host { get; private set; }
        public int Port { get; private set; }
    }
}
