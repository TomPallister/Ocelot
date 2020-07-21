namespace Ocelot.UnitTests.Requester
{
    using Microsoft.Extensions.DependencyInjection;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Logging;
    using Ocelot.Requester;
    using Ocelot.Requester.QoS;
    using Shouldly;
    using System.Net.Http;
    using Xunit;

    public class QoSFactoryTests
    {
        private QoSFactory _factory;
        private ServiceCollection _services;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;

        public QoSFactoryTests()
        {
            _services = new ServiceCollection();
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            var provider = _services.BuildServiceProvider();
            _factory = new QoSFactory(provider, _loggerFactory.Object);
        }

        [Fact]
        public void should_return_error()
        {
            var downstreamRoute = new DownstreamRouteBuilder().Build();
            var handler = _factory.Get(downstreamRoute);
            handler.IsError.ShouldBeTrue();
            handler.Errors[0].ShouldBeOfType<UnableToFindQoSProviderError>();
        }

        [Fact]
        public void should_return_handler()
        {
            _services = new ServiceCollection();
            DelegatingHandler QosDelegatingHandlerDelegate(DownstreamRoute a, IOcelotLoggerFactory b) => new FakeDelegatingHandler();
            _services.AddSingleton<QosDelegatingHandlerDelegate>(QosDelegatingHandlerDelegate);
            var provider = _services.BuildServiceProvider();
            _factory = new QoSFactory(provider, _loggerFactory.Object);
            var downstreamRoute = new DownstreamRouteBuilder().Build();
            var handler = _factory.Get(downstreamRoute);
            handler.IsError.ShouldBeFalse();
            handler.Data.ShouldBeOfType<FakeDelegatingHandler>();
        }

        private class FakeDelegatingHandler : DelegatingHandler
        {
        }
    }
}
