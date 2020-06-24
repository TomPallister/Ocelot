namespace Ocelot.UnitTests.LoadBalancer
{
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.LoadBalancer.LoadBalancers;
    using Ocelot.Responses;
    using Ocelot.ServiceDiscovery.Providers;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class LeastConnectionCreatorTests
    {
        private readonly LeastConnectionCreator _creator;
        private readonly Mock<IServiceDiscoveryProvider> _serviceProvider;
        private DownstreamRoute _route;
        private Response<ILoadBalancer> _loadBalancer;
        private string _typeName;

        public LeastConnectionCreatorTests()
        {
            _creator = new LeastConnectionCreator();
            _serviceProvider = new Mock<IServiceDiscoveryProvider>();
        }
        
        [Fact]
        public void should_return_instance_of_expected_load_balancer_type()
        {
            var route = new DownstreamRouteBuilder()
                .WithServiceName("myService")
                .Build();

            this.Given(x => x.GivenARoute(route))
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenTheLoadBalancerIsReturned<LeastConnection>())
                .BDDfy();
        }
                
        [Fact]
        public void should_return_expected_name()
        {
            this.When(x => x.WhenIGetTheLoadBalancerTypeName())
                .Then(x => x.ThenTheLoadBalancerTypeIs("LeastConnection"))
                .BDDfy();
        }

        private void GivenARoute(DownstreamRoute route)
        {
            _route = route;
        }

        private void WhenIGetTheLoadBalancer()
        {
            _loadBalancer = _creator.Create(_route, _serviceProvider.Object);
        }
        
        private void WhenIGetTheLoadBalancerTypeName()
        {
            _typeName = _creator.Type;
        }

        private void ThenTheLoadBalancerIsReturned<T>()
            where T : ILoadBalancer
        {
            _loadBalancer.Data.ShouldBeOfType<T>();
        }

        private void ThenTheLoadBalancerTypeIs(string type)
        {
            _typeName.ShouldBe(type);
        }
    }
}
