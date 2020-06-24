namespace Ocelot.UnitTests.Multiplexing
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Middleware;
    using Ocelot.Multiplexer;
    using Ocelot.Responses;
    using Ocelot.UnitTests.Responder;
    using Shouldly;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class UserDefinedResponseAggregatorTests
    {
        private readonly UserDefinedResponseAggregator _aggregator;
        private readonly Mock<IDefinedAggregatorProvider> _provider;
        private Route _route;
        private List<HttpContext> _contexts;
        private HttpContext _context;

        public UserDefinedResponseAggregatorTests()
        {
            _provider = new Mock<IDefinedAggregatorProvider>();
            _aggregator = new UserDefinedResponseAggregator(_provider.Object);
        }

        [Fact]
        public void should_call_aggregator()
        {
            var route = new RouteBuilder().Build();

            var context = new DefaultHttpContext();

            var contextA = new DefaultHttpContext();
            contextA.Items.UpsertDownstreamResponse(new DownstreamResponse(new StringContent("Tom"), HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "some reason"));

            var contextB = new DefaultHttpContext();
            contextB.Items.UpsertDownstreamResponse(new DownstreamResponse(new StringContent("Laura"), HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "some reason"));

            var contexts = new List<HttpContext>()
            {
                contextA,
                contextB,
            };

            this.Given(_ => GivenTheProviderReturnsAggregator())
                .And(_ => GivenRoute(route))
                .And(_ => GivenContexts(contexts))
                .And(_ => GivenContext(context))
                .When(_ => WhenIAggregate())
                .Then(_ => ThenTheProviderIsCalled())
                .And(_ => ThenTheContentIsCorrect())
                .BDDfy();
        }

        [Fact]
        public void should_not_find_aggregator()
        {
            var route = new RouteBuilder().Build();

            var context = new DefaultHttpContext();

            var contextA = new DefaultHttpContext();
            contextA.Items.UpsertDownstreamResponse(new DownstreamResponse(new StringContent("Tom"), HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "some reason"));

            var contextB = new DefaultHttpContext();
            contextB.Items.UpsertDownstreamResponse(new DownstreamResponse(new StringContent("Laura"), HttpStatusCode.OK, new List<KeyValuePair<string, IEnumerable<string>>>(), "some reason"));

            var contexts = new List<HttpContext>()
            {
                contextA,
                contextB,
            };

            this.Given(_ => GivenTheProviderReturnsError())
                .And(_ => GivenRoute(route))
                .And(_ => GivenContexts(contexts))
                .And(_ => GivenContext(context))
                .When(_ => WhenIAggregate())
                .Then(_ => ThenTheProviderIsCalled())
                .And(_ => ThenTheErrorIsReturned())
                .BDDfy();
        }

        private void ThenTheErrorIsReturned()
        {
            _context.Items.Errors().Count.ShouldBeGreaterThan(0);
            _context.Items.Errors().Count.ShouldBe(1);
        }

        private void GivenTheProviderReturnsError()
        {
            _provider.Setup(x => x.Get(It.IsAny<Route>())).Returns(new ErrorResponse<IDefinedAggregator>(new AnyError()));
        }

        private async Task ThenTheContentIsCorrect()
        {
            var content = await _context.Items.DownstreamResponse().Content.ReadAsStringAsync();
            content.ShouldBe("Tom, Laura");
        }

        private void ThenTheProviderIsCalled()
        {
            _provider.Verify(x => x.Get(_route), Times.Once);
        }

        private void GivenContext(HttpContext context)
        {
            _context = context;
        }

        private void GivenContexts(List<HttpContext> contexts)
        {
            _contexts = contexts;
        }

        private async Task WhenIAggregate()
        {
            await _aggregator.Aggregate(_route, _context, _contexts);
        }

        private void GivenTheProviderReturnsAggregator()
        {
            var aggregator = new TestDefinedAggregator();
            _provider.Setup(x => x.Get(It.IsAny<Route>())).Returns(new OkResponse<IDefinedAggregator>(aggregator));
        }

        private void GivenRoute(Route route)
        {
            _route = route;
        }

        public class TestDefinedAggregator : IDefinedAggregator
        {
            public async Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
            {
                var tom = await responses[0].Items.DownstreamResponse().Content.ReadAsStringAsync();
                var laura = await responses[1].Items.DownstreamResponse().Content.ReadAsStringAsync();
                var content = $"{tom}, {laura}";
                var headers = responses.SelectMany(x => x.Items.DownstreamResponse().Headers).ToList();
                return new DownstreamResponse(new StringContent(content), HttpStatusCode.OK, headers, "some reason");
            }
        }
    }
}
