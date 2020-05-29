namespace Ocelot.UnitTests.Configuration
{
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Configuration.Creator;
    using Ocelot.Configuration.File;
    using Shouldly;
    using System.Collections.Generic;
    using System.Net.Http;
    using TestStack.BDDfy;
    using Values;
    using Xunit;

    public class AggregatesCreatorTests
    {
        private readonly AggregatesCreator _creator;
        private readonly Mock<IUpstreamTemplatePatternCreator> _utpCreator;
        private FileConfiguration _fileConfiguration;
        private List<Route> _routes;
        private List<Route> _result;
        private UpstreamPathTemplate _aggregate1Utp;
        private UpstreamPathTemplate _aggregate2Utp;

        public AggregatesCreatorTests()
        {
            _utpCreator = new Mock<IUpstreamTemplatePatternCreator>();
            _creator = new AggregatesCreator(_utpCreator.Object);
        }

        [Fact]
        public void should_return_no_aggregates()
        {
            var fileConfig = new FileConfiguration
            {
                Aggregates = new List<FileAggregateRoute>
                {
                    new FileAggregateRoute
                    {
                        RouteKeys = new List<string>{"key1"}
                    }
                }
            };
            var routes = new List<Route>();

            this.Given(_ => GivenThe(fileConfig))
                .And(_ => GivenThe(routes))
                .When(_ => WhenICreate())
                .Then(_ => TheUtpCreatorIsNotCalled())
                .And(_ => ThenTheResultIsNotNull())
                .And(_ => ThenTheResultIsEmpty())
                .BDDfy();
        }

        [Fact]
        public void should_create_aggregates()
        {
            var fileConfig = new FileConfiguration
            {
                Aggregates = new List<FileAggregateRoute>
                {
                    new FileAggregateRoute
                    {
                        RouteKeys = new List<string>{"key1", "key2"},
                        UpstreamHost = "hosty",
                        UpstreamPathTemplate = "templatey",
                        Aggregator = "aggregatory",
                        RouteIsCaseSensitive = true
                    },
                    new FileAggregateRoute
                    {
                        RouteKeys = new List<string>{"key3", "key4"},
                        UpstreamHost = "hosty",
                        UpstreamPathTemplate = "templatey",
                        Aggregator = "aggregatory",
                        RouteIsCaseSensitive = true
                    }
                }
            };

            var routes = new List<Route>
            {
                new RouteBuilder().WithDownstreamRoute(new DownstreamRouteBuilder().WithKey("key1").Build()).Build(),
                new RouteBuilder().WithDownstreamRoute(new DownstreamRouteBuilder().WithKey("key2").Build()).Build(),
                new RouteBuilder().WithDownstreamRoute(new DownstreamRouteBuilder().WithKey("key3").Build()).Build(),
                new RouteBuilder().WithDownstreamRoute(new DownstreamRouteBuilder().WithKey("key4").Build()).Build()
            };

            this.Given(_ => GivenThe(fileConfig))
                .And(_ => GivenThe(routes))
                .And(_ => GivenTheUtpCreatorReturns())
                .When(_ => WhenICreate())
                .Then(_ => ThenTheUtpCreatorIsCalledCorrectly())
                .And(_ => ThenTheAggregatesAreCreated())
                .BDDfy();
        }

        private void ThenTheAggregatesAreCreated()
        {
            _result.ShouldNotBeNull();
            _result.Count.ShouldBe(2);

            _result[0].UpstreamHttpMethod.ShouldContain(x => x == HttpMethod.Get);
            _result[0].UpstreamHost.ShouldBe(_fileConfiguration.Aggregates[0].UpstreamHost);
            _result[0].UpstreamTemplatePattern.ShouldBe(_aggregate1Utp);
            _result[0].Aggregator.ShouldBe(_fileConfiguration.Aggregates[0].Aggregator);
            _result[0].DownstreamRoute.ShouldContain(x => x == _routes[0].DownstreamRoute[0]);
            _result[0].DownstreamRoute.ShouldContain(x => x == _routes[1].DownstreamRoute[0]);

            _result[1].UpstreamHttpMethod.ShouldContain(x => x == HttpMethod.Get);
            _result[1].UpstreamHost.ShouldBe(_fileConfiguration.Aggregates[1].UpstreamHost);
            _result[1].UpstreamTemplatePattern.ShouldBe(_aggregate2Utp);
            _result[1].Aggregator.ShouldBe(_fileConfiguration.Aggregates[1].Aggregator);
            _result[1].DownstreamRoute.ShouldContain(x => x == _routes[2].DownstreamRoute[0]);
            _result[1].DownstreamRoute.ShouldContain(x => x == _routes[3].DownstreamRoute[0]);
        }

        private void ThenTheUtpCreatorIsCalledCorrectly()
        {
            _utpCreator.Verify(x => x.Create(_fileConfiguration.Aggregates[0]), Times.Once);
            _utpCreator.Verify(x => x.Create(_fileConfiguration.Aggregates[1]), Times.Once);
        }

        private void GivenTheUtpCreatorReturns()
        {
            _aggregate1Utp = new UpstreamPathTemplateBuilder().Build();
            _aggregate2Utp = new UpstreamPathTemplateBuilder().Build();

            _utpCreator.SetupSequence(x => x.Create(It.IsAny<IRoute>()))
                .Returns(_aggregate1Utp)
                .Returns(_aggregate2Utp);
        }

        private void ThenTheResultIsEmpty()
        {
            _result.Count.ShouldBe(0);
        }

        private void ThenTheResultIsNotNull()
        {
            _result.ShouldNotBeNull();
        }

        private void TheUtpCreatorIsNotCalled()
        {
            _utpCreator.Verify(x => x.Create(It.IsAny<FileAggregateRoute>()), Times.Never);
        }

        private void GivenThe(FileConfiguration fileConfiguration)
        {
            _fileConfiguration = fileConfiguration;
        }

        private void GivenThe(List<Route> routes)
        {
            _routes = routes;
        }

        private void WhenICreate()
        {
            _result = _creator.Create(_fileConfiguration, _routes);
        }
    }
}
