namespace Ocelot.UnitTests.Requester
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Logging;
    using Ocelot.Requester;
    using Ocelot.Requester.QoS;
    using Ocelot.Responses;
    using Responder;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using TestStack.BDDfy;
    using Xunit;

    public class DelegatingHandlerHandlerProviderFactoryTests
    {
        private DelegatingHandlerHandlerFactory _factory;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;
        private readonly Mock<IOcelotLogger> _logger;
        private DownstreamRoute _downstreamRoute;
        private HttpContext _httpContext;
        private Response<List<Func<DelegatingHandler>>> _result;
        private readonly Mock<IQoSFactory> _qosFactory;
        private readonly Mock<ITracingHandlerFactory> _tracingFactory;
        private IServiceProvider _serviceProvider;
        private readonly IServiceCollection _services;
        private readonly QosDelegatingHandlerDelegate _qosDelegate;

        public DelegatingHandlerHandlerProviderFactoryTests()
        {
            _qosDelegate = (a, b) => new FakeQoSHandler();
            _tracingFactory = new Mock<ITracingHandlerFactory>();
            _qosFactory = new Mock<IQoSFactory>();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<DelegatingHandlerHandlerFactory>()).Returns(_logger.Object);
            _services = new ServiceCollection();
            _services.AddSingleton(_qosDelegate);
        }

        [Fact]
        public void should_follow_ordering_add_specifics()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithDelegatingHandlers(new List<string>
                {
                    "FakeDelegatingHandler",
                    "FakeDelegatingHandlerTwo"
                })
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandlerThree, FakeDelegatingHandlerFour>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(6))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerThree>(0))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerFour>(1))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(2))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerTwo>(3))
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(4))
                .And(x => ThenHandlerAtPositionIs<FakeQoSHandler>(5))
                .BDDfy();
        }

        [Fact]
        public void should_follow_ordering_order_specifics_and_globals()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithDelegatingHandlers(new List<string>
                {
                    "FakeDelegatingHandlerTwo",
                    "FakeDelegatingHandler",
                    "FakeDelegatingHandlerFour"
                })
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandlerFour, FakeDelegatingHandlerThree>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(6))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerThree>(0)) //first because global not in config
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerTwo>(1)) //first from config
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(2)) //second from config
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerFour>(3)) //third from config (global)
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(4))
                .And(x => ThenHandlerAtPositionIs<FakeQoSHandler>(5))
                .BDDfy();
        }

        [Fact]
        public void should_follow_ordering_order_specifics()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithDelegatingHandlers(new List<string>
                {
                    "FakeDelegatingHandlerTwo",
                    "FakeDelegatingHandler"
                })
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandlerThree, FakeDelegatingHandlerFour>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(6))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerThree>(0))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerFour>(1))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerTwo>(2))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(3))
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(4))
                .And(x => ThenHandlerAtPositionIs<FakeQoSHandler>(5))
                .BDDfy();
        }

        [Fact]
        public void should_follow_ordering_order_and_only_add_specifics_in_config()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithDelegatingHandlers(new List<string>
                {
                    "FakeDelegatingHandler",
                })
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandlerThree, FakeDelegatingHandlerFour>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(5))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerThree>(0))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerFour>(1))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(2))
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(3))
                .And(x => ThenHandlerAtPositionIs<FakeQoSHandler>(4))
                .BDDfy();
        }

        [Fact]
        public void should_follow_ordering_dont_add_specifics()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(4))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(0))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerTwo>(1))
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(2))
                .And(x => ThenHandlerAtPositionIs<FakeQoSHandler>(3))
                .BDDfy();
        }

        [Fact]
        public void should_apply_re_route_specific()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, false, true, int.MaxValue))
                .WithDelegatingHandlers(new List<string>
                {
                    "FakeDelegatingHandler",
                    "FakeDelegatingHandlerTwo"
                })
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(2))
                .And(x => ThenTheDelegatesAreAddedCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_attach_http_context_when_necessary()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, false, int.MaxValue))
                .WithDelegatingHandlers(new List<string>
                {
                    "FakeHandlerWithHttpContext",
                })
                .WithLoadBalancerKey("")
                .Build();

            _httpContext = new DefaultHttpContext();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeHandlerWithHttpContext, FakeDelegatingHandler>())
                .When(x => WhenIGet())
                .Then(x => ThenDelegateHasHttpContext())
                .BDDfy();
        }

        [Fact]
        public void should_all_from_all_routes_provider_and_qos()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, false, true, int.MaxValue)).WithLoadBalancerKey("").Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(3))
                .And(x => ThenTheDelegatesAreAddedCorrectly())
                .And(x => ThenItIsQosHandler(2))
                .BDDfy();
        }

        [Fact]
        public void should_return_provider_with_no_delegates()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, false, true, int.MaxValue)).WithLoadBalancerKey("").Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheServiceProviderReturnsNothing())
                .When(x => WhenIGet())
                .Then(x => ThenNoDelegatesAreInTheProvider())
                .BDDfy();
        }

        [Fact]
        public void should_return_provider_with_qos_delegate()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, false, true, int.MaxValue)).WithLoadBalancerKey("").Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheServiceProviderReturnsNothing())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(1))
                .And(x => ThenItIsQosHandler(0))
                .BDDfy();
        }

        [Fact]
        public void should_return_provider_with_qos_delegate_when_timeout_value_set()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, false, true, int.MaxValue)).WithLoadBalancerKey("").Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturns(new FakeQoSHandler()))
                .And(x => GivenTheServiceProviderReturnsNothing())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(1))
                .And(x => ThenItIsQosHandler(0))
                .BDDfy();
        }

        [Fact]
        public void should_log_error_and_return_no_qos_provider_delegate_when_qos_factory_returns_error()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturnsError())
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(4))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(0))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerTwo>(1))
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(2))
                .And(x => ThenHandlerAtPositionIs<NoQosDelegatingHandler>(3))
                .And(_ => ThenTheWarningIsLogged())
                .BDDfy();
        }

        [Fact]
        public void should_log_error_and_return_no_qos_provider_delegate_when_qos_factory_returns_null()
        {
            var qosOptions = new QoSOptionsBuilder()
                .WithTimeoutValue(1)
                .WithDurationOfBreak(1)
                .WithExceptionsAllowedBeforeBreaking(1)
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(true, true, true, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .Build();

            this.Given(x => GivenTheFollowingRequest(route))
                .And(x => GivenTheQosFactoryReturnsNull())
                .And(x => GivenTheTracingFactoryReturns())
                .And(x => GivenTheServiceProviderReturnsGlobalDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .And(x => GivenTheServiceProviderReturnsSpecificDelegatingHandlers<FakeDelegatingHandler, FakeDelegatingHandlerTwo>())
                .When(x => WhenIGet())
                .Then(x => ThenThereIsDelegatesInProvider(4))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandler>(0))
                .And(x => ThenHandlerAtPositionIs<FakeDelegatingHandlerTwo>(1))
                .And(x => ThenHandlerAtPositionIs<FakeTracingHandler>(2))
                .And(x => ThenHandlerAtPositionIs<NoQosDelegatingHandler>(3))
                .And(_ => ThenTheWarningIsLogged())
                .BDDfy();
        }

        private void ThenTheWarningIsLogged()
        {
            _logger.Verify(x => x.LogWarning($"Route {_downstreamRoute.UpstreamPathTemplate} specifies use QoS but no QosHandler found in DI container. Will use not use a QosHandler, please check your setup!"), Times.Once);
        }

        private void ThenHandlerAtPositionIs<T>(int pos)
            where T : DelegatingHandler
        {
            var delegates = _result.Data;
            var del = delegates[pos].Invoke();
            del.ShouldBeOfType<T>();
        }

        private void GivenTheTracingFactoryReturns()
        {
            _tracingFactory
                .Setup(x => x.Get())
                .Returns(new FakeTracingHandler());
        }

        private void GivenTheServiceProviderReturnsGlobalDelegatingHandlers<TOne, TTwo>()
            where TOne : DelegatingHandler
            where TTwo : DelegatingHandler
        {
            _services.AddTransient<TOne>();
            _services.AddTransient<GlobalDelegatingHandler>(s =>
            {
                var service = s.GetService<TOne>();
                return new GlobalDelegatingHandler(service);
            });
            _services.AddTransient<TTwo>();
            _services.AddTransient<GlobalDelegatingHandler>(s =>
            {
                var service = s.GetService<TTwo>();
                return new GlobalDelegatingHandler(service);
            });
        }

        private void GivenTheServiceProviderReturnsSpecificDelegatingHandlers<TOne, TTwo>()
            where TOne : DelegatingHandler
            where TTwo : DelegatingHandler
        {
            _services.AddTransient<DelegatingHandler, TOne>();
            _services.AddTransient<DelegatingHandler, TTwo>();
        }

        private void GivenTheServiceProviderReturnsNothing()
        {
            _serviceProvider = _services.BuildServiceProvider();
        }

        private void ThenAnErrorIsReturned()
        {
            _result.IsError.ShouldBeTrue();
        }

        private void ThenTheDelegatesAreAddedCorrectly()
        {
            var delegates = _result.Data;

            var del = delegates[0].Invoke();
            var handler = (FakeDelegatingHandler)del;
            handler.Order.ShouldBe(1);

            del = delegates[1].Invoke();
            var handlerTwo = (FakeDelegatingHandlerTwo)del;
            handlerTwo.Order.ShouldBe(2);
        }

        private void GivenTheQosFactoryReturns(DelegatingHandler handler)
        {
            _qosFactory
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>()))
                .Returns(new OkResponse<DelegatingHandler>(handler));
        }

        private void GivenTheQosFactoryReturnsError()
        {
            _qosFactory
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>()))
                .Returns(new ErrorResponse<DelegatingHandler>(new AnyError()));
        }

        private void GivenTheQosFactoryReturnsNull()
        {
            _qosFactory
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>()))
                .Returns((ErrorResponse<DelegatingHandler>)null);
        }

        private void ThenItIsQosHandler(int i)
        {
            var delegates = _result.Data;
            var del = delegates[i].Invoke();
            del.ShouldBeOfType<FakeQoSHandler>();
        }

        private void ThenThereIsDelegatesInProvider(int count)
        {
            _result.ShouldNotBeNull();
            _result.Data.Count.ShouldBe(count);
        }

        private void GivenTheFollowingRequest(DownstreamRoute request)
        {
            _downstreamRoute = request;
        }

        private void WhenIGet()
        {
            _serviceProvider = _services.BuildServiceProvider();
            _factory = new DelegatingHandlerHandlerFactory(_tracingFactory.Object, _qosFactory.Object, _serviceProvider, _loggerFactory.Object);
            _result = _factory.Get(_downstreamRoute, _httpContext);
        }

        private void ThenNoDelegatesAreInTheProvider()
        {
            _result.ShouldNotBeNull();
            _result.Data.Count.ShouldBe(0);
        }

        private void ThenDelegateHasHttpContext()
        {
            var delegates = _result.Data;

            var del = delegates[1].Invoke();
            var handler = (FakeHandlerWithHttpContext)del;
            handler.HttpContext.ShouldBe(_httpContext);
        }
    }

    internal class FakeTracingHandler : DelegatingHandler, ITracingHandler
    {
    }

    internal class FakeQoSHandler : DelegatingHandler
    {
    }

    internal class FakeHandlerWithHttpContext : DelegatingHandler, IDelegatingHandlerWithHttpContext
    {
        public HttpContext HttpContext { get; set; }
    }
}
