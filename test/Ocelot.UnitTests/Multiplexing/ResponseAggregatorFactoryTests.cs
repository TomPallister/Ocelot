namespace Ocelot.UnitTests.Multiplexing
{
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Multiplexer;
    using Shouldly;
    using TestStack.BDDfy;
    using Xunit;

    public class ResponseAggregatorFactoryTests
    {
        private readonly InMemoryResponseAggregatorFactory _factory;
        private Mock<IDefinedAggregatorProvider> _provider;
        private Route _route;
        private IResponseAggregator _aggregator;

        public ResponseAggregatorFactoryTests()
        {
            _provider = new Mock<IDefinedAggregatorProvider>();
            _aggregator = new SimpleJsonResponseAggregator();
            _factory = new InMemoryResponseAggregatorFactory(_provider.Object, _aggregator);
        }

        [Fact]
        public void should_return_simple_json_aggregator()
        {
            var route = new RouteBuilder()
                .Build();

            this.Given(_ => GivenRoute(route))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheAggregatorIs<SimpleJsonResponseAggregator>())
                .BDDfy();
        }

        [Fact]
        public void should_return_user_defined_aggregator()
        {
            var route = new RouteBuilder()
                .WithAggregator("doesntmatter")
                .Build();

            this.Given(_ => GivenRoute(route))
                .When(_ => WhenIGet())
                .Then(_ => ThenTheAggregatorIs<UserDefinedResponseAggregator>())
                .BDDfy();
        }

        private void GivenRoute(Route route)
        {
            _route = route;
        }

        private void WhenIGet()
        {
            _aggregator = _factory.Get(_route);
        }

        private void ThenTheAggregatorIs<T>()
        {
            _aggregator.ShouldBeOfType<T>();
        }
    }
}
