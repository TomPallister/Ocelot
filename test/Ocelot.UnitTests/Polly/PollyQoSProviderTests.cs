namespace Ocelot.UnitTests.Polly
{
    using Moq;
    using Ocelot.Configuration.Builder;
    using Ocelot.Logging;
    using Provider.Polly;
    using Shouldly;
    using Xunit;

    public class PollyQoSProviderTests
    {
        [Fact]
        public void should_build()
        {
            var options = new QoSOptionsBuilder()
                .WithTimeoutValue(100)
                .WithExceptionsAllowedBeforeBreaking(1)
                .WithDurationOfBreak(200)
                .Build();
            var route = new DownstreamRouteBuilder().WithQosOptions(options)
                .Build();
            var factory = new Mock<IOcelotLoggerFactory>();
            var pollyQoSProvider = new PollyQoSProvider(route, factory.Object);
            pollyQoSProvider.CircuitBreaker.ShouldNotBeNull();
        }
    }
}
