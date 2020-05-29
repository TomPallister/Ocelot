namespace Ocelot.UnitTests.Request.Mapper
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Ocelot.Request.Mapper;
    using Ocelot.Responses;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using Ocelot.Configuration;
    using Ocelot.Configuration.Builder;
    using TestStack.BDDfy;
    using Xunit;

    public class RequestMapperTests
    {
        private readonly HttpContext _httpContext;
        private readonly HttpRequest _inputRequest;

        private readonly RequestMapper _requestMapper;

        private Response<HttpRequestMessage> _mappedRequest;

        private List<KeyValuePair<string, StringValues>> _inputHeaders = null;

        private DownstreamRoute _downstreamRoute;

        public RequestMapperTests()
        {
            _httpContext = new DefaultHttpContext();
            _inputRequest = _httpContext.Request;
            _requestMapper = new RequestMapper();
        }

        [Theory]
        [InlineData("https", "my.url:123", "/abc/DEF", "?a=1&b=2", "https://my.url:123/abc/DEF?a=1&b=2")]
        [InlineData("http", "blah.com", "/d ef", "?abc=123", "http://blah.com/d%20ef?abc=123")] // note! the input is encoded when building the input request
        [InlineData("http", "myusername:mypassword@abc.co.uk", null, null, "http://myusername:mypassword@abc.co.uk/")]
        [InlineData("http", "點看.com", null, null, "http://xn--c1yn36f.com/")]
        [InlineData("http", "xn--c1yn36f.com", null, null, "http://xn--c1yn36f.com/")]
        public void Should_map_valid_request_uri(string scheme, string host, string path, string queryString, string expectedUri)
        {
            this.Given(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasScheme(scheme))
                .And(_ => GivenTheInputRequestHasHost(host))
                .And(_ => GivenTheInputRequestHasPath(path))
                .And(_ => GivenTheInputRequestHasQueryString(queryString))
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasUri(expectedUri))
                .BDDfy();
        }

        [Theory]
        [InlineData("ftp", "google.com", "/abc/DEF", "?a=1&b=2")]
        public void Should_error_on_unsupported_request_uri(string scheme, string host, string path, string queryString)
        {
            this.Given(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasScheme(scheme))
                .And(_ => GivenTheInputRequestHasHost(host))
                .And(_ => GivenTheInputRequestHasPath(path))
                .And(_ => GivenTheInputRequestHasQueryString(queryString))
                .When(_ => WhenMapped())
                .Then(_ => ThenAnErrorIsReturned())
                .And(_ => ThenTheMappedRequestIsNull())
                .BDDfy();
        }

        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("WHATEVER")]
        public void Should_map_method(string method)
        {
            this.Given(_ => GivenTheInputRequestHasMethod(method))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasMethod(method))
                .BDDfy();
        }

        [Theory]
        [InlineData("", "GET")]
        [InlineData(null, "GET")]
        [InlineData("POST", "POST")]
        public void Should_use_downstream_route_method_if_set(string input, string expected)
        {
            this.Given(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheDownstreamRouteMethodIs(input))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasMethod(expected))
                .BDDfy();
        }

        [Fact]
        public void Should_map_all_headers()
        {
            this.Given(_ => GivenTheInputRequestHasHeaders())
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasEachHeader())
                .BDDfy();
        }

        [Fact]
        public void Should_handle_no_headers()
        {
            this.Given(_ => GivenTheInputRequestHasNoHeaders())
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasNoHeaders())
                .BDDfy();
        }

        [Fact]
        public void Should_map_content()
        {
            this.Given(_ => GivenTheInputRequestHasContent("This is my content"))
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasContent("This is my content"))
                .BDDfy();
        }

        [Fact]
        public void Should_handle_no_content()
        {
            this.Given(_ => GivenTheInputRequestHasNullContent())
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasNoContent())
                .BDDfy();
        }

        [Fact]
        public void Should_handle_no_content_type()
        {
            this.Given(_ => GivenTheInputRequestHasNoContentType())
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasNoContent())
                .BDDfy();
        }

        [Fact]
        public void Should_handle_no_content_length()
        {
            this.Given(_ => GivenTheInputRequestHasNoContentLength())
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasNoContent())
                .BDDfy();
        }

        [Fact]
        public void Should_map_content_headers()
        {
            byte[] md5bytes = new byte[0];
            using (var md5 = MD5.Create())
            {
                md5bytes = md5.ComputeHash(Encoding.UTF8.GetBytes("some md5"));
            }

            this.Given(_ => GivenTheInputRequestHasContent("This is my content"))
                .And(_ => GivenTheContentTypeIs("application/json"))
                .And(_ => GivenTheContentEncodingIs("gzip, compress"))
                .And(_ => GivenTheContentLanguageIs("english"))
                .And(_ => GivenTheContentLocationIs("/my-receipts/38"))
                .And(_ => GivenTheContentRangeIs("bytes 1-2/*"))
                .And(_ => GivenTheContentDispositionIs("inline"))
                .And(_ => GivenTheContentMD5Is(md5bytes))
                .And(_ => GivenTheInputRequestHasMethod("GET"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasContentTypeHeader("application/json"))
                .And(_ => ThenTheMappedRequestHasContentEncodingHeader("gzip", "compress"))
                .And(_ => ThenTheMappedRequestHasContentLanguageHeader("english"))
                .And(_ => ThenTheMappedRequestHasContentLocationHeader("/my-receipts/38"))
                .And(_ => ThenTheMappedRequestHasContentMD5Header(md5bytes))
                .And(_ => ThenTheMappedRequestHasContentRangeHeader())
                .And(_ => ThenTheMappedRequestHasContentDispositionHeader("inline"))
                .And(_ => ThenTheMappedRequestHasContentSize("This is my content".Length))
                .And(_ => ThenTheContentHeadersAreNotAddedToNonContentHeaders())
                .BDDfy();
        }

        [Fact]
        public void should_not_add_content_headers()
        {
            this.Given(_ => GivenTheInputRequestHasContent("This is my content"))
                .And(_ => GivenTheContentTypeIs("application/json"))
                .And(_ => GivenTheInputRequestHasMethod("POST"))
                .And(_ => GivenTheInputRequestHasAValidUri())
                .And(_ => GivenTheDownstreamRoute())
                .When(_ => WhenMapped())
                .Then(_ => ThenNoErrorIsReturned())
                .And(_ => ThenTheMappedRequestHasContentTypeHeader("application/json"))
                .And(_ => ThenTheMappedRequestHasContentSize("This is my content".Length))
                .And(_ => ThenTheOtherContentTypeHeadersAreNotMapped())
                .BDDfy();
        }

        private void GivenTheDownstreamRouteMethodIs(string input)
        {
            _downstreamRoute = new DownstreamRouteBuilder()
                .WithDownStreamHttpMethod(input)
                .WithDownstreamHttpVersion(new Version("1.1")).Build();
        }

        private void GivenTheDownstreamRoute()
        {
            _downstreamRoute = new DownstreamRouteBuilder()
                .WithDownstreamHttpVersion(new Version("1.1")).Build();
        }

        private void GivenTheInputRequestHasNoContentLength()
        {
            _inputRequest.ContentLength = null;
        }

        private void GivenTheInputRequestHasNoContentType()
        {
            _inputRequest.ContentType = null;
        }


        private void ThenTheContentHeadersAreNotAddedToNonContentHeaders()
        {
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-Disposition");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-ContentMD5");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-ContentRange");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-ContentLanguage");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-ContentEncoding");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-ContentLocation");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-Length");
            _mappedRequest.Data.Headers.ShouldNotContain(x => x.Key == "Content-Type");
        }

        private void ThenTheOtherContentTypeHeadersAreNotMapped()
        {
            _mappedRequest.Data.Content.Headers.ContentDisposition.ShouldBeNull();
            _mappedRequest.Data.Content.Headers.ContentMD5.ShouldBeNull();
            _mappedRequest.Data.Content.Headers.ContentRange.ShouldBeNull();
            _mappedRequest.Data.Content.Headers.ContentLanguage.ShouldBeEmpty();
            _mappedRequest.Data.Content.Headers.ContentEncoding.ShouldBeEmpty();
            _mappedRequest.Data.Content.Headers.ContentLocation.ShouldBeNull();
        }

        private void ThenTheMappedRequestHasContentDispositionHeader(string expected)
        {
            _mappedRequest.Data.Content.Headers.ContentDisposition.DispositionType.ShouldBe(expected);
        }

        private void GivenTheContentDispositionIs(string input)
        {
            _inputRequest.Headers.Add("Content-Disposition", input);
        }

        private void ThenTheMappedRequestHasContentMD5Header(byte[] expected)
        {
            _mappedRequest.Data.Content.Headers.ContentMD5.ShouldBe(expected);
        }

        private void GivenTheContentMD5Is(byte[] input)
        {
            var base64 = Convert.ToBase64String(input);
            _inputRequest.Headers.Add("Content-MD5", base64);
        }

        private void ThenTheMappedRequestHasContentRangeHeader()
        {
            _mappedRequest.Data.Content.Headers.ContentRange.From.ShouldBe(1);
            _mappedRequest.Data.Content.Headers.ContentRange.To.ShouldBe(2);
        }

        private void GivenTheContentRangeIs(string input)
        {
            _inputRequest.Headers.Add("Content-Range", input);
        }

        private void ThenTheMappedRequestHasContentLocationHeader(string expected)
        {
            _mappedRequest.Data.Content.Headers.ContentLocation.OriginalString.ShouldBe(expected);
        }

        private void GivenTheContentLocationIs(string input)
        {
            _inputRequest.Headers.Add("Content-Location", input);
        }

        private void ThenTheMappedRequestHasContentLanguageHeader(string expected)
        {
            _mappedRequest.Data.Content.Headers.ContentLanguage.First().ShouldBe(expected);
        }

        private void GivenTheContentLanguageIs(string input)
        {
            _inputRequest.Headers.Add("Content-Language", input);
        }

        private void ThenTheMappedRequestHasContentEncodingHeader(string expected, string expectedTwo)
        {
            _mappedRequest.Data.Content.Headers.ContentEncoding.ToArray()[0].ShouldBe(expected);
            _mappedRequest.Data.Content.Headers.ContentEncoding.ToArray()[1].ShouldBe(expectedTwo);
        }

        private void GivenTheContentEncodingIs(string input)
        {
            _inputRequest.Headers.Add("Content-Encoding", input);
        }

        private void GivenTheContentTypeIs(string contentType)
        {
            _inputRequest.ContentType = contentType;
        }

        private void ThenTheMappedRequestHasContentTypeHeader(string expected)
        {
            _mappedRequest.Data.Content.Headers.ContentType.MediaType.ShouldBe(expected);
        }

        private void ThenTheMappedRequestHasContentSize(long expected)
        {
            _mappedRequest.Data.Content.Headers.ContentLength.ShouldBe(expected);
        }

        private void GivenTheInputRequestHasMethod(string method)
        {
            _inputRequest.Method = method;
        }

        private void GivenTheInputRequestHasScheme(string scheme)
        {
            _inputRequest.Scheme = scheme;
        }

        private void GivenTheInputRequestHasHost(string host)
        {
            _inputRequest.Host = new HostString(host);
        }

        private void GivenTheInputRequestHasPath(string path)
        {
            if (path != null)
            {
                _inputRequest.Path = path;
            }
        }

        private void GivenTheInputRequestHasQueryString(string querystring)
        {
            if (querystring != null)
            {
                _inputRequest.QueryString = new QueryString(querystring);
            }
        }

        private void GivenTheInputRequestHasAValidUri()
        {
            GivenTheInputRequestHasScheme("http");
            GivenTheInputRequestHasHost("www.google.com");
        }

        private void GivenTheInputRequestHasHeaders()
        {
            _inputHeaders = new List<KeyValuePair<string, StringValues>>()
            {
                new KeyValuePair<string, StringValues>("abc", new StringValues(new string[]{"123","456" })),
                new KeyValuePair<string, StringValues>("def", new StringValues(new string[]{"789","012" })),
            };

            foreach (var inputHeader in _inputHeaders)
            {
                _inputRequest.Headers.Add(inputHeader);
            }
        }

        private void GivenTheInputRequestHasNoHeaders()
        {
            _inputRequest.Headers.Clear();
        }

        private void GivenTheInputRequestHasContent(string content)
        {
            _inputRequest.Body = new MemoryStream(Encoding.UTF8.GetBytes(content));
        }

        private void GivenTheInputRequestHasNullContent()
        {
            _inputRequest.Body = null;
        }

        private async Task WhenMapped()
        {
            _mappedRequest = await _requestMapper.Map(_inputRequest, _downstreamRoute);
        }

        private void ThenNoErrorIsReturned()
        {
            _mappedRequest.IsError.ShouldBeFalse();
        }

        private void ThenAnErrorIsReturned()
        {
            _mappedRequest.IsError.ShouldBeTrue();
        }

        private void ThenTheMappedRequestHasUri(string expectedUri)
        {
            _mappedRequest.Data.RequestUri.OriginalString.ShouldBe(expectedUri);
        }

        private void ThenTheMappedRequestHasMethod(string expectedMethod)
        {
            _mappedRequest.Data.Method.ToString().ShouldBe(expectedMethod);
        }

        private void ThenTheMappedRequestHasEachHeader()
        {
            _mappedRequest.Data.Headers.Count().ShouldBe(_inputHeaders.Count);
            foreach (var header in _mappedRequest.Data.Headers)
            {
                var inputHeader = _inputHeaders.First(h => h.Key == header.Key);
                inputHeader.ShouldNotBeNull();
                inputHeader.Value.Count().ShouldBe(header.Value.Count());
                foreach (var inputHeaderValue in inputHeader.Value)
                {
                    header.Value.Any(v => v == inputHeaderValue);
                }
            }
        }

        private void ThenTheMappedRequestHasNoHeaders()
        {
            _mappedRequest.Data.Headers.Count().ShouldBe(0);
        }

        private void ThenTheMappedRequestHasContent(string expectedContent)
        {
            _mappedRequest.Data.Content.ReadAsStringAsync().GetAwaiter().GetResult().ShouldBe(expectedContent);
        }

        private void ThenTheMappedRequestHasNoContent()
        {
            _mappedRequest.Data.Content.ShouldBeNull();
        }

        private void ThenTheMappedRequestIsNull()
        {
            _mappedRequest.Data.ShouldBeNull();
        }
    }
}
