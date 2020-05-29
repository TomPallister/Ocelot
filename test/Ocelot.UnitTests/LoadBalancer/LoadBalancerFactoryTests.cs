using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.LoadBalancer.LoadBalancers;
using Ocelot.Responses;
using Ocelot.ServiceDiscovery;
using Ocelot.ServiceDiscovery.Providers;
using Shouldly;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ocelot.Infrastructure.RequestData;
using Ocelot.Middleware;
using Ocelot.Values;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.LoadBalancer
{
    using System;
    using Microsoft.AspNetCore.Http;

    public class LoadBalancerFactoryTests
    {
        private DownstreamRoute _route;
        private readonly LoadBalancerFactory _factory;
        private Response<ILoadBalancer> _result;
        private readonly Mock<IServiceDiscoveryProviderFactory> _serviceProviderFactory;
        private readonly IEnumerable<ILoadBalancerCreator> _loadBalancerCreators;
        private readonly Mock<IServiceDiscoveryProvider> _serviceProvider;
        private ServiceProviderConfiguration _serviceProviderConfig;

        public LoadBalancerFactoryTests()
        {
            _serviceProviderFactory = new Mock<IServiceDiscoveryProviderFactory>();
            _serviceProvider = new Mock<IServiceDiscoveryProvider>();
            _loadBalancerCreators = new ILoadBalancerCreator[]
            {
                new FakeLoadBalancerCreator<FakeLoadBalancerOne>(),
                new FakeLoadBalancerCreator<FakeLoadBalancerTwo>(),
                new FakeLoadBalancerCreator<FakeNoLoadBalancer>(nameof(NoLoadBalancer)),
                new BrokenLoadBalancerCreator<BrokenLoadBalancer>(),
            };
            _factory = new LoadBalancerFactory(_serviceProviderFactory.Object, _loadBalancerCreators);
        }

        [Fact]
        public void should_return_no_load_balancer_by_default()
        {
            var route = new DownstreamRouteBuilder()
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => GivenAServiceProviderConfig(new ServiceProviderConfigurationBuilder().Build()))
                .And(x => x.GivenTheServiceProviderFactoryReturns())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenTheLoadBalancerIsReturned<FakeNoLoadBalancer>())
                .BDDfy();
        }

        [Fact]
        public void should_return_matching_load_balancer()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeLoadBalancerTwo", "", 0))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => GivenAServiceProviderConfig(new ServiceProviderConfigurationBuilder().Build()))
                .And(x => x.GivenTheServiceProviderFactoryReturns())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenTheLoadBalancerIsReturned<FakeLoadBalancerTwo>())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_response_if_cannot_find_load_balancer_creator()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("DoesntExistLoadBalancer", "", 0))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => GivenAServiceProviderConfig(new ServiceProviderConfigurationBuilder().Build()))
                .And(x => x.GivenTheServiceProviderFactoryReturns())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenAnErrorResponseIsReturned())
                .And(x => x.ThenTheErrorMessageIsCorrect())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_response_if_creator_errors()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("BrokenLoadBalancer", "", 0))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => GivenAServiceProviderConfig(new ServiceProviderConfigurationBuilder().Build()))
                .And(x => x.GivenTheServiceProviderFactoryReturns())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenAnErrorResponseIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_call_service_provider()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeLoadBalancerOne", "", 0))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => GivenAServiceProviderConfig(new ServiceProviderConfigurationBuilder().Build()))
                .And(x => x.GivenTheServiceProviderFactoryReturns())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenTheServiceProviderIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_return_error_response_when_call_to_service_provider_fails()
        {
            var route = new DownstreamRouteBuilder()
                .WithLoadBalancerOptions(new LoadBalancerOptions("FakeLoadBalancerOne", "", 0))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            this.Given(x => x.GivenARoute(route))
                .And(x => GivenAServiceProviderConfig(new ServiceProviderConfigurationBuilder().Build()))
                .And(x => x.GivenTheServiceProviderFactoryFails())
                .When(x => x.WhenIGetTheLoadBalancer())
                .Then(x => x.ThenAnErrorResponseIsReturned())
                .BDDfy();
        }

        private void GivenAServiceProviderConfig(ServiceProviderConfiguration serviceProviderConfig)
        {
            _serviceProviderConfig = serviceProviderConfig;
        }

        private void GivenTheServiceProviderFactoryReturns()
        {
            _serviceProviderFactory
                .Setup(x => x.Get(It.IsAny<ServiceProviderConfiguration>(), It.IsAny<DownstreamRoute>()))
                .Returns(new OkResponse<IServiceDiscoveryProvider>(_serviceProvider.Object));
        }

        private void GivenTheServiceProviderFactoryFails()
        {
            _serviceProviderFactory
                .Setup(x => x.Get(It.IsAny<ServiceProviderConfiguration>(), It.IsAny<DownstreamRoute>()))
                .Returns(new ErrorResponse<IServiceDiscoveryProvider>(new CannotFindDataError("For tests")));
        }

        private void ThenTheServiceProviderIsCalledCorrectly()
        {
            _serviceProviderFactory
                .Verify(x => x.Get(It.IsAny<ServiceProviderConfiguration>(), It.IsAny<DownstreamRoute>()), Times.Once);
        }

        private void GivenARoute(DownstreamRoute route)
        {
            _route = route;
        }

        private void WhenIGetTheLoadBalancer()
        {
            _result = _factory.Get(_route, _serviceProviderConfig);
        }

        private void ThenTheLoadBalancerIsReturned<T>()
        {
            _result.Data.ShouldBeOfType<T>();
        }

        private void ThenAnErrorResponseIsReturned()
        {
            _result.IsError.ShouldBeTrue();
        }

        private void ThenTheErrorMessageIsCorrect()
        {
            _result.Errors[0].Message.ShouldBe("Could not find load balancer creator for Type: DoesntExistLoadBalancer, please check your config specified the correct load balancer and that you have registered a class with the same name.");
        }

        private class FakeLoadBalancerCreator<T> : ILoadBalancerCreator
            where T : ILoadBalancer, new()
        {

            public FakeLoadBalancerCreator()
            {
                Type = typeof(T).Name;
            }

            public FakeLoadBalancerCreator(string type)
            {
                Type = type;
            }

            public Response<ILoadBalancer> Create(DownstreamRoute route, IServiceDiscoveryProvider serviceProvider)
            {
                return new OkResponse<ILoadBalancer>(new T());
            }
            
            public string Type { get; }
        }

        private class BrokenLoadBalancerCreator<T> : ILoadBalancerCreator
            where T : ILoadBalancer, new()
        {
            public BrokenLoadBalancerCreator()
            {
                Type = typeof(T).Name;
            }

            public Response<ILoadBalancer> Create(DownstreamRoute route, IServiceDiscoveryProvider serviceProvider)
            {
                return new ErrorResponse<ILoadBalancer>(new ErrorInvokingLoadBalancerCreator(new Exception()));
            }

            public string Type { get; }
        }

        private class FakeLoadBalancerOne : ILoadBalancer
        {
            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                throw new System.NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                throw new System.NotImplementedException();
            }
        }

        private class FakeLoadBalancerTwo : ILoadBalancer
        {
            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                throw new System.NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                throw new System.NotImplementedException();
            }
        }

        private class FakeNoLoadBalancer : ILoadBalancer
        {
            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                throw new System.NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                throw new System.NotImplementedException();
            }
        }

        private class BrokenLoadBalancer : ILoadBalancer
        {
            public Task<Response<ServiceHostAndPort>> Lease(HttpContext httpContext)
            {
                throw new System.NotImplementedException();
            }

            public void Release(ServiceHostAndPort hostAndPort)
            {
                throw new System.NotImplementedException();
            }
        }

    }
}
