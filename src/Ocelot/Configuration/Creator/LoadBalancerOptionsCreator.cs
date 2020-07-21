using Ocelot.Configuration.File;

namespace Ocelot.Configuration.Creator
{
    public class LoadBalancerOptionsCreator : ILoadBalancerOptionsCreator
    {
        public LoadBalancerOptions Create(FileLoadBalancerOptions options)
        {
            return new LoadBalancerOptionsBuilder()
                .WithType(options.Type)
                .WithKey(options.Key)
                .WithExpiryInMs(options.Expiry)
                .Build();
        }
    }
}
