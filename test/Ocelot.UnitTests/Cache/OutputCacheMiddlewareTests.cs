namespace Ocelot.UnitTests.Cache
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Cache;
    using Ocelot.Cache.Middleware;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using Ocelot.DownstreamRouteFinder;
    using Ocelot.DownstreamRouteFinder.UrlMatcher;
    using Ocelot.Logging;
    using Ocelot.Middleware;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using TestStack.BDDfy;
    using Xunit;

    public class OutputCacheMiddlewareTests
    {
        private readonly Mock<IOcelotCache<CachedResponse>> _cache;
        private readonly Mock<IOcelotLoggerFactory> _loggerFactory;
        private Mock<IOcelotLogger> _logger;
        private OutputCacheMiddleware _middleware;
        private readonly DownstreamContext _downstreamContext;
        private readonly OcelotRequestDelegate _next;
        private readonly ICacheKeyGenerator _cacheKeyGenerator;
        private CachedResponse _response;


        public OutputCacheMiddlewareTests()
        {
            _cache = new Mock<IOcelotCache<CachedResponse>>();
            _downstreamContext = new DownstreamContext(new DefaultHttpContext());
            _loggerFactory = new Mock<IOcelotLoggerFactory>();
            _logger = new Mock<IOcelotLogger>();
            _cacheKeyGenerator = new CacheKeyGenerator();
            _loggerFactory.Setup(x => x.CreateLogger<OutputCacheMiddleware>()).Returns(_logger.Object);
            _next = context => Task.CompletedTask;
            _downstreamContext.DownstreamRequest = new Ocelot.Request.Middleware.DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "https://some.url/blah?abcd=123"));
        }

        [Fact]
        public void should_returned_cached_item_when_it_is_in_cache()
        {
            var headers = new Dictionary<string, IEnumerable<string>>
            {
                { "test", new List<string> { "test" } }
            };

            var contentHeaders = new Dictionary<string, IEnumerable<string>>
            {
                { "content-type", new List<string> { "application/json" } }
            };

            var cachedResponse = new CachedResponse(HttpStatusCode.OK, headers, "", contentHeaders, "some reason");
            this.Given(x => x.GivenThereIsACachedResponse(cachedResponse))
                .And(x => x.GivenTheDownstreamRouteIs())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheCacheGetIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_returned_cached_item_when_it_is_in_cache_expires_header()
        {
            var contentHeaders = new Dictionary<string, IEnumerable<string>>
            {
                { "Expires", new List<string> { "-1" } }
            };

            var cachedResponse = new CachedResponse(HttpStatusCode.OK, new Dictionary<string, IEnumerable<string>>(), "", contentHeaders, "some reason");
            this.Given(x => x.GivenThereIsACachedResponse(cachedResponse))
                .And(x => x.GivenTheDownstreamRouteIs())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheCacheGetIsCalledCorrectly())
                .BDDfy();
        }

        [Fact]
        public void should_continue_with_pipeline_and_cache_response()
        {
            this.Given(x => x.GivenResponseIsNotCached(new HttpResponseMessage()))
                .And(x => x.GivenTheDownstreamRouteIs())
                .When(x => x.WhenICallTheMiddleware())
                .Then(x => x.ThenTheCacheAddIsCalledCorrectly())
                .BDDfy();
        }

        private void WhenICallTheMiddleware()
        {
            _middleware = new OutputCacheMiddleware(_next, _loggerFactory.Object, _cache.Object, _cacheKeyGenerator);
            _middleware.Invoke(_downstreamContext).GetAwaiter().GetResult();
        }

        private void GivenThereIsACachedResponse(CachedResponse response)
        {
            _response = response;
            _cache
              .Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>()))
              .Returns(_response);
        }

        private void GivenResponseIsNotCached(HttpResponseMessage responseMessage)
        {
            _downstreamContext.DownstreamResponse = new DownstreamResponse(responseMessage);
        }

        private void GivenTheDownstreamRouteIs()
        {
            var reRoute = new ReRouteBuilder()
                .WithDownstreamReRoute(new DownstreamReRouteBuilder()
                    .WithIsCached(true)
                    .WithCacheOptions(new CacheOptions(100, "kanken", null))
                    .WithUpstreamHttpMethod(new List<string> { "Get" })
                    .Build())
                .WithUpstreamHttpMethod(new List<string> { "Get" })
                .Build();

            var downstreamRoute = new DownstreamRoute(new List<PlaceholderNameAndValue>(), reRoute);

            _downstreamContext.TemplatePlaceholderNameAndValues = downstreamRoute.TemplatePlaceholderNameAndValues;
            _downstreamContext.DownstreamReRoute = downstreamRoute.ReRoute.DownstreamReRoute[0];
        }

        private void ThenTheCacheGetIsCalledCorrectly()
        {
            _cache
                .Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private void ThenTheCacheAddIsCalledCorrectly()
        {
            _cache
                .Verify(x => x.Add(It.IsAny<string>(), It.IsAny<CachedResponse>(), It.IsAny<TimeSpan>(), It.IsAny<string>()), Times.Once);
        }
    }
}
