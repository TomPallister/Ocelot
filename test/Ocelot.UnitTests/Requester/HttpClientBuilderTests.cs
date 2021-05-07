using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Moq;
using Ocelot.Configuration;
using Ocelot.Configuration.Builder;
using Ocelot.DownstreamRouteFinder.Middleware;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Request.Middleware;
using Ocelot.Requester;
using Ocelot.Responses;
using Shouldly;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Requester
{
    public class HttpClientBuilderTests : IDisposable
    {
        private HttpClientBuilder _builder;
        private readonly Mock<IDelegatingHandlerHandlerFactory> _factory;
        private IHttpClient _httpClient;
        private HttpResponseMessage _response;
        private HttpContext _context;
        private readonly Mock<IHttpClientCache> _cacheHandlers;
        private readonly Mock<IOcelotLogger> _logger;
        private int _count;
        private IWebHost _host;
        private IHttpClient _againHttpClient;
        private IHttpClient _firstHttpClient;
        private MemoryHttpClientCache _realCache;

        public HttpClientBuilderTests()
        {
            _cacheHandlers = new Mock<IHttpClientCache>();
            _logger = new Mock<IOcelotLogger>();
            _factory = new Mock<IDelegatingHandlerHandlerFactory>();
            _builder = new HttpClientBuilder(_factory.Object, _cacheHandlers.Object, _logger.Object);
        }

        [Fact]
        public void should_build_http_client()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            this.Given(x => GivenTheFactoryReturns())
                .And(x => GivenARequest(route))
                .When(x => WhenIBuild())
                .Then(x => ThenTheHttpClientShouldNotBeNull())
                .BDDfy();
        }

        [Fact]
        public void should_get_from_cache()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            this.Given(x => GivenARealCache())
                .And(x => GivenTheFactoryReturns())
                .And(x => GivenARequest(route))
                .And(x => WhenIBuildTheFirstTime())
                .And(x => WhenISave())
                .And(x => WhenIBuildAgain())
                .And(x => WhenISave())
                .When(x => WhenIBuildAgain())
                .Then(x => ThenTheHttpClientIsFromTheCache())
                .BDDfy();
        }

        [Fact]
        public void should_get_from_cache_with_different_query_string()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            this.Given(x => GivenARealCache())
                .And(x => GivenTheFactoryReturns())
                .And(x => GivenARequest(route, "http://wwww.someawesomewebsite.com/woot?badman=1"))
                .And(x => WhenIBuildTheFirstTime())
                .And(x => WhenISave())
                .And(x => WhenIBuildAgain())
                .And(x => GivenARequest(route, "http://wwww.someawesomewebsite.com/woot?badman=2"))
                .And(x => WhenISave())
                .When(x => WhenIBuildAgain())
                .Then(x => ThenTheHttpClientIsFromTheCache())
                .BDDfy();
        }

        [Fact]
        public void should_not_get_from_cache_with_different_query_string()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var routeA = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithContainsQueryString(true).WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            var routeB = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithContainsQueryString(true).WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            this.Given(x => GivenARealCache())
                .And(x => GivenTheFactoryReturns())
                .And(x => GivenARequest(routeA, "http://wwww.someawesomewebsite.com/woot?badman=1"))
                .And(x => WhenIBuildTheFirstTime())
                .And(x => WhenISave())
                .And(x => WhenIBuildAgain())
                .And(x => GivenARequest(routeB, "http://wwww.someawesomewebsite.com/woot?badman=2"))
                .And(x => WhenISave())
                .When(x => WhenIBuildAgain())
                .Then(x => ThenTheHttpClientIsNotFromTheCache())
                .BDDfy();
        }

        [Fact]
        public void should_log_if_ignoring_ssl_errors()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .WithDangerousAcceptAnyServerCertificateValidator(true)
                .Build();

            this.Given(x => GivenTheFactoryReturns())
                .And(x => GivenARequest(route))
                .When(x => WhenIBuild())
                .Then(x => ThenTheHttpClientShouldNotBeNull())
                .Then(x => ThenTheDangerousAcceptAnyServerCertificateValidatorWarningIsLogged())
                .BDDfy();
        }

        [Fact]
        public void should_call_delegating_handlers_in_order()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            var fakeOne = new FakeDelegatingHandler();
            var fakeTwo = new FakeDelegatingHandler();

            var handlers = new List<Func<DelegatingHandler>>()
            {
                () => fakeOne,
                () => fakeTwo
            };

            this.Given(x => GivenTheFactoryReturns(handlers))
                .And(x => GivenARequest(route))
                .And(x => WhenIBuild())
                .When(x => WhenICallTheClient())
                .Then(x => ThenTheFakeAreHandledInOrder(fakeOne, fakeTwo))
                .And(x => ThenSomethingIsReturned())
                .BDDfy();
        }

        [Fact]
        public void should_re_use_cookies_from_container()
        {
            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, true, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            this.Given(_ => GivenADownstreamService())
                .And(_ => GivenARequest(route))
                .And(_ => GivenTheFactoryReturnsNothing())
                .And(_ => WhenIBuild())
                .And(_ => WhenICallTheClient("http://localhost:5003"))
                .And(_ => ThenTheCookieIsSet())
                .And(_ => GivenTheClientIsCached())
                .And(_ => WhenIBuild())
                .When(_ => WhenICallTheClient("http://localhost:5003"))
                .Then(_ => ThenTheResponseIsOk())
                .BDDfy();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        [InlineData("PATCH")]
        public void should_add_verb_to_cache_key(string verb)
        {
            var downstreamUrl = "http://localhost:5012/";

            var method = new HttpMethod(verb);

            var qosOptions = new QoSOptionsBuilder()
                .Build();

            var route = new DownstreamRouteBuilder()
                .WithQosOptions(qosOptions)
                .WithHttpHandlerOptions(new HttpHandlerOptions(false, false, false, true, int.MaxValue))
                .WithLoadBalancerKey("")
                .WithUpstreamPathTemplate(new UpstreamPathTemplateBuilder().WithOriginalValue("").Build())
                .WithQosOptions(new QoSOptionsBuilder().Build())
                .Build();

            this.Given(_ => GivenADownstreamService())
                .And(_ => GivenARequestWithAUrlAndMethod(route, downstreamUrl, method))
                .And(_ => GivenTheFactoryReturnsNothing())
                .And(_ => WhenIBuild())
                .And(_ => GivenCacheIsCalledWithExpectedKey($"{method.ToString()}:{downstreamUrl}"))
                .BDDfy();
        }

        private void GivenARealCache()
        {
            _realCache = new MemoryHttpClientCache();
            _builder = new HttpClientBuilder(_factory.Object, _realCache, _logger.Object);
        }

        private void ThenTheHttpClientIsFromTheCache()
        {
            _againHttpClient.ShouldBe(_firstHttpClient);
        }

        private void ThenTheHttpClientIsNotFromTheCache()
        {
            _againHttpClient.ShouldNotBe(_firstHttpClient);
        }

        private void WhenISave()
        {
            _builder.Save();
        }

        private void GivenCacheIsCalledWithExpectedKey(string expectedKey)
        {
            _cacheHandlers.Verify(x => x.Get(It.IsAny<DownstreamRoute>()), Times.Once);
        }

        private void ThenTheDangerousAcceptAnyServerCertificateValidatorWarningIsLogged()
        {
            _logger.Verify(x => x.LogWarning($"You have ignored all SSL warnings by using DangerousAcceptAnyServerCertificateValidator for this DownstreamRoute, UpstreamPathTemplate: {_context.Items.DownstreamRoute().UpstreamPathTemplate}, DownstreamPathTemplate: {_context.Items.DownstreamRoute().DownstreamPathTemplate}"), Times.Once);
        }

        private void GivenTheClientIsCached()
        {
            _cacheHandlers.Setup(x => x.Get(It.IsAny<DownstreamRoute>())).Returns(_httpClient);
        }

        private void ThenTheCookieIsSet()
        {
            _response.Headers.TryGetValues("Set-Cookie", out var test).ShouldBeTrue();
        }

        private void WhenICallTheClient(string url)
        {
            _response = _httpClient
                .SendAsync(new HttpRequestMessage(HttpMethod.Get, url))
                .GetAwaiter()
                .GetResult();
        }

        private void ThenTheResponseIsOk()
        {
            _response.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        private void GivenADownstreamService()
        {
            _host = new WebHostBuilder()
                .UseUrls("http://localhost:5003")
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .Configure(app =>
                {
                    app.Run(context =>
                    {
                        if (_count == 0)
                        {
                            context.Response.Cookies.Append("test", "0");
                            context.Response.StatusCode = 200;
                            _count++;
                            return Task.CompletedTask;
                        }

                        if (_count == 1)
                        {
                            if (context.Request.Cookies.TryGetValue("test", out var cookieValue) || context.Request.Headers.TryGetValue("Set-Cookie", out var headerValue))
                            {
                                context.Response.StatusCode = 200;
                                return Task.CompletedTask;
                            }

                            context.Response.StatusCode = 500;
                        }

                        return Task.CompletedTask;
                    });
                })
                .Build();

            _host.Start();
        }

        private void GivenARequest(DownstreamRoute downstream)
        {
            GivenARequest(downstream, "http://localhost:5003");
        }

        private void GivenARequest(DownstreamRoute downstream, string downstreamUrl)
        {
            GivenARequestWithAUrlAndMethod(downstream, downstreamUrl, HttpMethod.Get);
        }

        private void GivenARequestWithAUrlAndMethod(DownstreamRoute downstream, string url, HttpMethod method)
        {
            _context = new DefaultHttpContext();
            _context.Items.UpsertDownstreamRoute(downstream);
            _context.Items.UpsertDownstreamRequest(new DownstreamRequest(new HttpRequestMessage() { RequestUri = new Uri(url), Method = method }));
        }

        private void ThenSomethingIsReturned()
        {
            _response.ShouldNotBeNull();
        }

        private void WhenICallTheClient()
        {
            _response = _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "http://test.com")).GetAwaiter().GetResult();
        }

        private void ThenTheFakeAreHandledInOrder(FakeDelegatingHandler fakeOne, FakeDelegatingHandler fakeTwo)
        {
            fakeOne.TimeCalled.ShouldBeGreaterThan(fakeTwo.TimeCalled);
        }

        private void GivenTheFactoryReturns()
        {
            var handlers = new List<Func<DelegatingHandler>>() { () => new FakeDelegatingHandler() };

            _factory
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>(), It.IsAny<HttpContext>()))
                .Returns(new OkResponse<List<Func<DelegatingHandler>>>(handlers));
        }

        private void GivenTheFactoryReturnsNothing()
        {
            var handlers = new List<Func<DelegatingHandler>>();

            _factory
                .Setup(x => x.Get(It.IsAny<DownstreamRoute>(), It.IsAny<HttpContext>()))
                .Returns(new OkResponse<List<Func<DelegatingHandler>>>(handlers));
        }

        private void GivenTheFactoryReturns(List<Func<DelegatingHandler>> handlers)
        {
            _factory
               .Setup(x => x.Get(It.IsAny<DownstreamRoute>(), It.IsAny<HttpContext>()))
               .Returns(new OkResponse<List<Func<DelegatingHandler>>>(handlers));
        }

        private void WhenIBuild()
        {
            _httpClient = _builder.Create(_context.Items.DownstreamRoute(), It.IsAny<HttpContext>());
        }

        private void WhenIBuildTheFirstTime()
        {
            _firstHttpClient = _builder.Create(_context.Items.DownstreamRoute(), It.IsAny<HttpContext>());
        }

        private void WhenIBuildAgain()
        {
            _builder = new HttpClientBuilder(_factory.Object, _realCache, _logger.Object);
            _againHttpClient = _builder.Create(_context.Items.DownstreamRoute(), It.IsAny<HttpContext>());
        }

        private void ThenTheHttpClientShouldNotBeNull()
        {
            _httpClient.ShouldNotBeNull();
        }

        public void Dispose()
        {
            _response?.Dispose();
            _host?.Dispose();
        }
    }
}
