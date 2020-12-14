using Ocelot.Middleware;

namespace Ocelot.UnitTests.RateLimit
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Caching.Memory;
    using Moq;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.Logging;
    using Ocelot.RateLimit;
    using Ocelot.RateLimit.Middleware;
    using Ocelot.Request.Middleware;
    using Shouldly;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Ocelot.Infrastructure.RequestData;
    using TestStack.BDDfy;
    using Xunit;
    using Ocelot.DownstreamRouteFinder.Middleware;

    public class ClientRateLimitMiddlewareTests
    {
        private int _responseStatusCode;
        private IRateLimitCounterHandler _rateLimitCounterHandler;
        private Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private Mock<IHttpContextAccessor> _httpContextAccessor;
        private readonly ClientRateLimitMiddleware _middleware;
        private RequestDelegate _next;
        private DownstreamResponse _downstreamResponse;
        private readonly string _url;

        public ClientRateLimitMiddlewareTests()
        {
            _url = "http://localhost:51879";
            var cacheEntryOptions = new MemoryCacheOptions();
            _rateLimitCounterHandler = new MemoryCacheRateLimitCounterHandler(new MemoryCache(cacheEntryOptions));
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _loggerFactory.Setup(x => x.CreateLogger<ClientRateLimitMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _httpContextAccessor = new Mock<IHttpContextAccessor>();
            _middleware = new ClientRateLimitMiddleware(_next, _loggerFactory.Object, _rateLimitCounterHandler, _httpContextAccessor.Object);
        }

        [Fact]
        public void should_call_middleware_and_ratelimiting()
        {
            var upstreamTemplate = new UpstreamPathTemplateBuilder().Build();

            var downstreamRoute = new DownstreamRouteBuilder()
                .WithEnableRateLimiting(true)
                .WithRateLimitOptions(new RateLimitOptions(true, "ClientId", () => new List<string>(), false, "", "", new RateLimitRule("1s", 100, 3), 429))
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .WithUpstreamPathTemplate(upstreamTemplate)
                .Build();

            var route = new RouteBuilder()
                .WithDownstreamRoute(downstreamRoute)
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            var downstreamRouteHolder = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(new List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue>(), route);

            this.Given(x => x.WhenICallTheMiddlewareMultipleTimes(2, downstreamRouteHolder))
                .Then(x => x.ThenThereIsNoDownstreamResponse())
                .When(x => x.WhenICallTheMiddlewareMultipleTimes(3, downstreamRouteHolder))
                .Then(x => x.ThenTheResponseIs429())
                .BDDfy();
        }

        [Fact]
        public void should_call_middleware_withWhitelistClient()
        {
            var downstreamRoute = new Ocelot.DownstreamRouteFinder.DownstreamRouteHolder(new List<Ocelot.DownstreamRouteFinder.UrlMatcher.PlaceholderNameAndValue>(),
                 new RouteBuilder()
                     .WithDownstreamRoute(new DownstreamRouteBuilder()
                         .WithEnableRateLimiting(true)
                         .WithRateLimitOptions(
                             new Ocelot.Configuration.RateLimitOptions(true, "ClientId", () => new List<string>() { "ocelotclient2" }, false, "", "", new RateLimitRule("1s", 100, 3), 429))
                         .WithUpstreamHttpMethod(new List<string> { "Get" })
                         .Build())
                     .WithUpstreamHttpMethod(new List<string> { "Get" })
                     .Build());

            this.Given(x => x.WhenICallTheMiddlewareWithWhiteClient(downstreamRoute))
                .Then(x => x.ThenThereIsNoDownstreamResponse())
                .BDDfy();
        }

        private void WhenICallTheMiddlewareMultipleTimes(int times, Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            var httpContexts = new List<HttpContext>();

            for (int i = 0; i < times; i++)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Response.Body = new FakeStream();
                httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
                httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);
                httpContext.Items.UpsertDownstreamRoute(downstreamRoute);
                var clientId = "ocelotclient1";
                var request = new HttpRequestMessage(new HttpMethod("GET"), _url);
                httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(request));
                httpContext.Request.Headers.TryAdd("ClientId", clientId);
                httpContexts.Add(httpContext);
            }

            var httpContextSeq = _httpContextAccessor.SetupSequence(x => x.HttpContext);
            httpContexts.ForEach(x => httpContextSeq.Returns(x));

            foreach (var httpContext in httpContexts)
            {
                _middleware.Invoke(httpContext).GetAwaiter().GetResult();
                var ds = httpContext.Items.DownstreamResponse();
                _downstreamResponse = ds;
            }
        }

        private void WhenICallTheMiddlewareWithWhiteClient(Ocelot.DownstreamRouteFinder.DownstreamRouteHolder downstreamRoute)
        {
            var clientId = "ocelotclient2";

            for (int i = 0; i < 10; i++)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Response.Body = new FakeStream();
                httpContext.Items.UpsertDownstreamRoute(downstreamRoute.Route.DownstreamRoute[0]);
                httpContext.Items.UpsertTemplatePlaceholderNameAndValues(downstreamRoute.TemplatePlaceholderNameAndValues);
                httpContext.Items.UpsertDownstreamRoute(downstreamRoute);
                var request = new HttpRequestMessage(new HttpMethod("GET"), _url);
                request.Headers.Add("ClientId", clientId);
                httpContext.Items.UpsertDownstreamRequest(new DownstreamRequest(request));
                httpContext.Request.Headers.TryAdd("ClientId", clientId);
                _middleware.Invoke(httpContext).GetAwaiter().GetResult();
                var ds = httpContext.Items.DownstreamResponse();
                _downstreamResponse = ds;
            }
        }

        private void ThenTheResponseIs429()
        {
            var code = (int)_downstreamResponse.StatusCode;
            code.ShouldBe(429);
        }

        private void ThenThereIsNoDownstreamResponse()
        {
            _downstreamResponse.ShouldBeNull();
        }
    }

    internal class FakeStream : Stream
    {
        public override void Flush()
        {
            //do nothing
            //throw new System.NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //do nothing
        }

        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite => true;
        public override long Length { get; }
        public override long Position { get; set; }
    }
}
