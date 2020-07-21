using Ocelot.Configuration;
using Ocelot.LoadBalancer.LoadBalancers;
using Shouldly;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    public class LoadBalancerOptionsTests
    {
        [Fact]
        public void should_default_to_no_load_balancer()
        {
            var options = new LoadBalancerOptionsBuilder().Build();
            options.Type.ShouldBe(nameof(NoLoadBalancer));
        }
    }
}
