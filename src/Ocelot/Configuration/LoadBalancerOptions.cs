using Ocelot.LoadBalancer.LoadBalancers;

namespace Ocelot.Configuration
{
    public class LoadBalancerOptions
    {
        public LoadBalancerOptions(string type, string key, int expiryInMs)
        {
            Type = string.IsNullOrWhiteSpace(type) ? nameof(NoLoadBalancer) : type;
            Key = key;
            ExpiryInMs = expiryInMs;
        }

        public string Type { get; }

        public string Key { get; }

        public int ExpiryInMs { get; }
    }
}
