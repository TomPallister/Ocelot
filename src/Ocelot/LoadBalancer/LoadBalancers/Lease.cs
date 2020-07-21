using Ocelot.Values;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class Lease
    {
        public Lease(ServiceHostAndPort hostAndPort, int connections)
        {
            HostAndPort = hostAndPort;
            Connections = connections;
        }

        public ServiceHostAndPort HostAndPort { get; private set; }
        public int Connections { get; private set; }
    }
}
