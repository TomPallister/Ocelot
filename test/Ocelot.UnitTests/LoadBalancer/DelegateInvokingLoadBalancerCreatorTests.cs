using System;
using System.Threading.Tasks;
using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Middleware;
using Ocelot.Responses;
using Ocelot.ServiceDiscovery.Providers;
using Ocelot.Values;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    using Microsoft.AspNetCore.Http;

    public class DelegateInvokingLoadBalancerCreatorTests
    {
        private DelegateInvokingLoadBalancerCreator<FakeLoadBalancer> _creator;
        private Func<DownstreamRoute, IServiceDiscoveryProvider, ILoadBalancer> _creatorFunc;
        private readonly Mock<IServiceDiscoveryProvider> _serviceProvider;
        private DownstreamRoute _route;
        private Response<ILoadBalancer> _loadBalancer;
        private string _typeName;

        public DelegateInvokingLoadBalancerCreatorTests()
        {
            _creatorFunc = (route, serviceDiscoveryProvider) =>
                new FakeLoadBalancer(route, serviceDiscoveryProvider);
            _creator = new DelegateInvokingLoadBalancerCreator<FakeLoadBalancer>(_creatorFunc);
            _serviceProvider = new Mock<IServiceDiscoveryProvider>();
        }

        [Fact]
        public void should_return_expected_name()
        {
            this.When(x => x.WhenIGetTheLoadBalancerTypeName())
                .Then(x => x.ThenTheLoadBalancerTypeIs("FakeLoadBalancer"))
                .BDDfy();
        }

        [Fact]
        public void should_return_result_of_specified_creator_func()
        {
            var route = new DownstreamRouteBuilder()
                .Build();

            this.Given(x => x.GivenARoute(route))
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenTheLoadBalancerIsReturned<FakeLoadBalancer>())
                .BDDfy();
        }

        [Fact]
        public void should_return_error()
        {
            var route = new DownstreamRouteBuilder()
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => x.GivenTheCreatorFuncThrows())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenAnErrorIsReturned())
                .BDDfy();
        }

        private void GivenTheCreatorFuncThrows()
        {
            _creatorFunc = (route, serviceDiscoveryProvider) => throw new Exception();

            _creator = new DelegateInvokingLoadBalancerCreator<FakeLoadBalancer>(_creatorFunc);
        }

        private void ThenAnErrorIsReturned()
        {
            _loadBalancer.IsError.ShouldBeTrue();
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

        private class FakeLoadBalancer : ILoadBalancer
        {
            public FakeLoadBalancer(DownstreamRoute downstreamRoute, IServiceDiscoveryProvider serviceDiscoveryProvider)
            {
                DownstreamRoute = downstreamRoute;
                ServiceDiscoveryProvider = serviceDiscoveryProvider;
            }

            public DownstreamRoute DownstreamRoute { get; }
            public IServiceDiscoveryProvider ServiceDiscoveryProvider { get; }

            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                throw new NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                throw new NotImplementedException();
            }
        }
    }
}
