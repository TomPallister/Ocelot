namespace Ocelot.UnitTests.Infrastructure
{
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Ocelot.Infrastructure;
    using Ocelot.Infrastructure.RequestData;
    using Ocelot.Middleware;
    using Ocelot.Request.Middleware;
    using Ocelot.Responses;
    using Shouldly;
    using System;
    using System.Net;
    using System.Net.Http;
    using Xunit;

    public class PlaceholdersTests
    {
        private readonly IPlaceholders _placeholders;
        private readonly Mock<IBaseUrlFinder> _finder;
        private readonly Mock<IRequestScopedDataRepository> _repo;
        private readonly Mock<IHttpContextAccessor> _accessor;

        public PlaceholdersTests()
        {
            _accessor = new Mock<IHttpContextAccessor>();
            _repo = new Mock<IRequestScopedDataRepository>();
            _finder = new Mock<IBaseUrlFinder>();
            _placeholders = new Placeholders(_finder.Object, _repo.Object, _accessor.Object);
        }

        [Fact]
        public void should_return_base_url()
        {
            var baseUrl = "http://www.bbc.co.uk";
            _finder.Setup(x => x.Find()).Returns(baseUrl);
            var result = _placeholders.Get("{BaseUrl}");
            result.Data.ShouldBe(baseUrl);
        }

        [Fact]
        public void should_return_remote_ip_address()
        {
            var httpContext = new DefaultHttpContext() { Connection = { RemoteIpAddress = IPAddress.Any } };
            _accessor.Setup(x => x.HttpContext).Returns(httpContext);
            var result = _placeholders.Get("{RemoteIpAddress}");
            result.Data.ShouldBe(httpContext.Connection.RemoteIpAddress.ToString());
        }

        [Fact]
        public void should_return_key_does_not_exist()
        {
            var result = _placeholders.Get("{Test}");
            result.IsError.ShouldBeTrue();
            result.Errors[0].Message.ShouldBe("Unable to find placeholder called {Test}");
        }

        [Fact]
        public void should_return_downstream_base_url_when_port_is_not_80_or_443()
        {
            var httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = new Uri("http://www.bbc.co.uk");
            var request = new DownstreamRequest(httpRequest);
            var result = _placeholders.Get("{DownstreamBaseUrl}", request);
            result.Data.ShouldBe("http://www.bbc.co.uk/");
        }

        [Fact]
        public void should_return_downstream_base_url_when_port_is_80_or_443()
        {
            var httpRequest = new HttpRequestMessage();
            httpRequest.RequestUri = new Uri("http://www.bbc.co.uk:123");
            var request = new DownstreamRequest(httpRequest);
            var result = _placeholders.Get("{DownstreamBaseUrl}", request);
            result.Data.ShouldBe("http://www.bbc.co.uk:123/");
        }

        [Fact]
        public void should_return_key_does_not_exist_for_http_request_message()
        {
            var request = new DownstreamRequest(new HttpRequestMessage(HttpMethod.Get, "http://west.com"));
            var result = _placeholders.Get("{Test}", request);
            result.IsError.ShouldBeTrue();
            result.Errors[0].Message.ShouldBe("Unable to find placeholder called {Test}");
        }

        [Fact]
        public void should_return_trace_id()
        {
            var traceId = "123";
            _repo.Setup(x => x.Get<string>("TraceId")).Returns(new OkResponse<string>(traceId));
            var result = _placeholders.Get("{TraceId}");
            result.Data.ShouldBe(traceId);
        }

        [Fact]
        public void should_return_ok_when_added()
        {
            var result = _placeholders.Add("{Test}", () => new OkResponse<string>("test"));
            result.IsError.ShouldBeFalse();
        }

        [Fact]
        public void should_return_ok_when_removed()
        {
            var result = _placeholders.Add("{Test}", () => new OkResponse<string>("test"));
            result = _placeholders.Remove("{Test}");
            result.IsError.ShouldBeFalse();
        }

        [Fact]
        public void should_return_error_when_added()
        {
            var result = _placeholders.Add("{Test}", () => new OkResponse<string>("test"));
            result = _placeholders.Add("{Test}", () => new OkResponse<string>("test"));
            result.IsError.ShouldBeTrue();
            result.Errors[0].Message.ShouldBe("Unable to add placeholder: {Test}, placeholder already exists");
        }

        [Fact]
        public void should_return_error_when_removed()
        {
            var result = _placeholders.Remove("{Test}");
            result.IsError.ShouldBeTrue();
            result.Errors[0].Message.ShouldBe("Unable to remove placeholder: {Test}, placeholder does not exists");
        }

        [Fact]
        public void should_return_upstreamHost()
        {
            var upstreamHost = "UpstreamHostA";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Add("Host", upstreamHost);
            _accessor.Setup(x => x.HttpContext).Returns(httpContext);
            var result = _placeholders.Get("{UpstreamHost}");
            result.Data.ShouldBe(upstreamHost);
        }

        [Fact]
        public void should_return_error_when_finding_upstbecause_Host_not_set()
        {
            var httpContext = new DefaultHttpContext();
            _accessor.Setup(x => x.HttpContext).Returns(httpContext);
            var result = _placeholders.Get("{UpstreamHost}");
            result.IsError.ShouldBeTrue();
        }

        [Fact]
        public void should_return_error_when_finding_upstream_host_because_exception_thrown()
        {
            _accessor.Setup(x => x.HttpContext).Throws(new Exception());
            var result = _placeholders.Get("{UpstreamHost}");
            result.IsError.ShouldBeTrue();
        }
    }
}
