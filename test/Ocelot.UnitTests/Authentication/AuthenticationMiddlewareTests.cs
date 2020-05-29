using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Ocelot.UnitTests.Authentication
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Authentication.Middleware;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using Shouldly;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class AuthenticationMiddlewareTests
    {
        private AuthenticationMiddleware _middleware;
        private readonly Mock<IOcelotLoggerFactory> _factory;
        private Mock<IOcelotLogger> _logger;
        private RequestDelegate _next;
        private HttpContext _httpContext;
        private Mock<IRequestScopedDataRepository> _repo;

        public AuthenticationMiddlewareTests()
        {
            _repo = new Mock<IRequestScopedDataRepository>();
            _httpContext = new DefaultHttpContext();
            _factory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _factory.Setup(x => x.CreateLogger<AuthenticationMiddleware>()).Returns(_logger.Object);
        }

        [Fact]
        public void should_call_next_middleware_if_route_is_not_authenticated()
        {
            this.Given(x => GivenTheDownStreamRouteIs(
                    new DownstreamRouteBuilder().WithUpstreamHttpMethod(new List<string> { "Get" }).Build()))
                .And(x => GivenTheTestServerPipelineIsConfigured())
                .When(x => WhenICallTheMiddleware())
                .Then(x => ThenTheUserIsAuthenticated())
                .BDDfy();
        }

        [Fact]
        public void should_call_next_middleware_if_route_is_using_options_method()
        {
            this.Given(x => GivenTheDownStreamRouteIs(
                    new DownstreamRouteBuilder()
                        .WithUpstreamHttpMethod(new List<string> { "Options" })
                        .WithIsAuthenticated(true)
                        .Build()))
                .And(x => GivenTheRequestIsUsingOptionsMethod())
                .When(x => WhenICallTheMiddleware())
                .Then(x => ThenTheUserIsAuthenticated())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _next = (context) =>
            {
                byte[] byteArray = Encoding.ASCII.GetBytes("The user is authenticated");
                var stream = new MemoryStream(byteArray);
                _httpContext.Response.Body = stream;
                return Task.CompletedTask;
            };
            _middleware = new AuthenticationMiddleware(_next, _factory.Object);
            _middleware.Invoke(_httpContext).GetAwaiter().GetResult();
        }

        private void GivenTheTestServerPipelineIsConfigured()
        {
            _next = (context) =>
            {
                byte[] byteArray = Encoding.ASCII.GetBytes("The user is authenticated");
                var stream = new MemoryStream(byteArray);
                _httpContext.Response.Body = stream;
                return Task.CompletedTask;
            };
        }

        private void GivenTheRequestIsUsingOptionsMethod()
        {
            _httpContext.Request.Method = "OPTIONS";
        }

        private void ThenTheUserIsAuthenticated()
        {
            var content = _httpContext.Response.Body.AsString();
            content.ShouldBe("The user is authenticated");
        }

        private void GivenTheDownStreamRouteIs(DownstreamRoute downstreamRoute)
        {
            _httpContext.Items.UpsertDownstreamRoute(downstreamRoute);
        }
    }

    public static class StreamExtensions
    {
        public static string AsString(this Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();
                return text;
            }
        }
    }
}
