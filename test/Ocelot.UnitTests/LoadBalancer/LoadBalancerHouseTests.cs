using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Middleware;
using Ocelot.Responses;
using Ocelot.Values;
using Shouldly;
using System;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    using Microsoft.AspNetCore.Http;

    public class LoadBalancerHouseTests
    {
        private DownstreamRoute _route;
        private ILoadBalancer _loadBalancer;
        private readonly LoadBalancerHouse _loadBalancerHouse;
        private Response<ILoadBalancer> _getResult;
        private readonly Mock<ILoadBalancerFactory> _factory;
        private readonly ServiceProviderConfiguration _serviceProviderConfig;

        public LoadBalancerHouseTests()
        {
            _factory = new Mock<ILoadBalancerFactory>();
            _loadBalancerHouse = new LoadBalancerHouse(_factory.Object);
            _serviceProviderConfig = new ServiceProviderConfiguration("myType", "myScheme", "myHost", 123, string.Empty, "configKey", 0);
        }

        [Fact]
        public void should_store_load_balancer_on_first_request()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerKey("test")
                .Build();

            this.Given(x => x.GivenThereIsALoadBalancer(route, new FakeLoadBalancer()))
                .Then(x => x.ThenItIsAdded())
                .BDDfy();
        }

        [Fact]
        public void should_not_store_load_balancer_on_second_request()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeLoadBalancer", "", 0))
                .WithLoadBalancerKey("test")
                .Build();

            this.Given(x => x.GivenThereIsALoadBalancer(route, new FakeLoadBalancer()))
                .When(x => x.WhenWeGetTheLoadBalancer(route))
                .Then(x => x.ThenItIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_store_load_balancers_by_key()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeLoadBalancer", "", 0))
                .WithLoadBalancerKey("test")
                .Build();

            var routeTwo = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeRoundRobinLoadBalancer", "", 0))
                .WithLoadBalancerKey("testtwo")
                .Build();

            this.Given(x => x.GivenThereIsALoadBalancer(route, new FakeLoadBalancer()))
                .And(x => x.GivenThereIsALoadBalancer(routeTwo, new FakeRoundRobinLoadBalancer()))
                .When(x => x.WhenWeGetTheLoadBalancer(route))
                .Then(x => x.ThenTheLoadBalancerIs<FakeLoadBalancer>())
                .When(x => x.WhenWeGetTheLoadBalancer(routeTwo))
                .Then(x => x.ThenTheLoadBalancerIs<FakeRoundRobinLoadBalancer>())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_if_exception()
        {
            var route = new DownstreamRouteBuilder().Build();

            this.When(x => x.WhenWeGetTheLoadBalancer(route))
            .Then(x => x.ThenAnErrorIsReturned())
            .BDDfy();
        }

        [Fact]
        public void should_get_new_load_balancer_if_route_load_balancer_has_changed()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeLoadBalancer", "", 0))
                .WithLoadBalancerKey("test")
                .Build();

            var routeTwo = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("LeastConnection", "", 0))
                .WithLoadBalancerKey("test")
                .Build();

            this.Given(x => x.GivenThereIsALoadBalancer(route, new FakeLoadBalancer()))
                .When(x => x.WhenWeGetTheLoadBalancer(route))
                .Then(x => x.ThenTheLoadBalancerIs<FakeLoadBalancer>())
                .When(x => x.WhenIGetTheRouteWithTheSameKeyButDifferentLoadBalancer(routeTwo))
                .Then(x => x.ThenTheLoadBalancerIs<LeastConnection>())
                .BDDfy();
        }

        private void WhenIGetTheRouteWithTheSameKeyButDifferentLoadBalancer(DownstreamRoute route)
        {
            _route = route;
            _factory.Setup(x => x.Get(_route, _serviceProviderConfig)).Returns(new OkResponse<ILoadBalancer>(new LeastConnection(null, null)));
            _getResult = _loadBalancerHouse.Get(_route, _serviceProviderConfig);
        }

        private void ThenAnErrorIsReturned()
        {
            _getResult.IsError.ShouldBeTrue();
            _getResult.Errors[0].ShouldBeOfType<UnableToFindLoadBalancerError>();
        }

        private void ThenTheLoadBalancerIs<T>()
        {
            _getResult.Data.ShouldBeOfType<T>();
        }

        private void ThenItIsAdded()
        {
            _getResult.IsError.ShouldBe(false);
            _getResult.ShouldBeOfType<OkResponse<ILoadBalancer>>();
            _getResult.Data.ShouldBe(_loadBalancer);
            _factory.Verify(x => x.Get(_route, _serviceProviderConfig), Times.Once);
        }

        private void GivenThereIsALoadBalancer(DownstreamRoute route, ILoadBalancer loadBalancer)
        {
            _route = route;
            _loadBalancer = loadBalancer;
            _factory.Setup(x => x.Get(_route, _serviceProviderConfig)).Returns(new OkResponse<ILoadBalancer>(loadBalancer));
            _getResult = _loadBalancerHouse.Get(route, _serviceProviderConfig);
        }

        private void WhenWeGetTheLoadBalancer(DownstreamRoute route)
        {
            _getResult = _loadBalancerHouse.Get(route, _serviceProviderConfig);
        }

        private void ThenItIsReturned()
        {
            _getResult.Data.ShouldBe(_loadBalancer);
            _factory.Verify(x => x.Get(_route, _serviceProviderConfig), Times.Once);
        }

        private class FakeLoadBalancer : ILoadBalancer
        {
            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                throw new NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                throw new NotImplementedException();
            }
        }

        private class FakeRoundRobinLoadBalancer : ILoadBalancer
        {
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
