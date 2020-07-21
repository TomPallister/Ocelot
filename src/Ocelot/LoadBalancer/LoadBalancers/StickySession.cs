using Ocelot.Values;
using System;

namespace Ocelot.LoadBalancer.LoadBalancers
{
    public class StickySession
    {
        public StickySession(ServiceHostAndPort hostAndPort, DateTime expiry, string key)
        {
            HostAndPort = hostAndPort;
            Expiry = expiry;
            Key = key;
        }

        public ServiceHostAndPort HostAndPort { get; }

        public DateTime Expiry { get; }

        public string Key { get; }
    }
}
