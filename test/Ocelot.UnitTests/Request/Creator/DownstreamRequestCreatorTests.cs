using Moq;
using Ocelot.Infrastructure;
using Ocelot.Request.Creator;
using Ocelot.Request.Middleware;
using Shouldly;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Request.Creator
{
    public class DownstreamRequestCreatorTests
    {
        private Mock<IFrameworkDescription> _framework;
        private DownstreamRequestCreator _downstreamRequestCreator;
        private HttpRequestMessage _request;
        private DownstreamRequest _result;

        public DownstreamRequestCreatorTests()
        {
            _framework = new Mock<IFrameworkDescription>();
            _downstreamRequestCreator = new DownstreamRequestCreator(_framework.Object);
        }

        [Fact]
        public void should_create_downstream_request()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.test.com");
            var content = new StringContent("test");
            request.Content = content;

            this.Given(_ => GivenTheFrameworkIs(""))
                .And(_ => GivenTheRequestIs(request))
                .When(_ => WhenICreate())
                .Then(_ => ThenTheDownstreamRequestHasABody())
                .BDDfy();
        }

        [Fact]
        public void should_remove_body_for_http_methods()
        {
            var methods = new List<HttpMethod> { HttpMethod.Get, HttpMethod.Head, HttpMethod.Delete, HttpMethod.Trace };
            var request = new HttpRequestMessage(HttpMethod.Get, "http://www.test.com");
            var content = new StringContent("test");
            request.Content = content;

            methods.ForEach(m =>
            {
                this.Given(_ => GivenTheFrameworkIs(".NET Framework"))
                    .And(_ => GivenTheRequestIs(request))
                    .When(_ => WhenICreate())
                    .Then(_ => ThenTheDownstreamRequestDoesNotHaveABody())
                    .BDDfy();
            });
        }

        private void GivenTheFrameworkIs(string framework)
        {
            _framework.Setup(x => x.Get()).Returns(framework);
        }

        private void GivenTheRequestIs(HttpRequestMessage request)
        {
            _request = request;
        }

        private void WhenICreate()
        {
            _result = _downstreamRequestCreator.Create(_request);
        }

        private async Task ThenTheDownstreamRequestHasABody()
        {
            _result.ShouldNotBeNull();
            _result.Method.ToLower().ShouldBe("get");
            _result.Scheme.ToLower().ShouldBe("http");
            _result.Host.ToLower().ShouldBe("www.test.com");
            var resultContent = await _result.ToHttpRequestMessage().Content.ReadAsStringAsync();
            resultContent.ShouldBe("test");
        }

        private void ThenTheDownstreamRequestDoesNotHaveABody()
        {
            _result.ShouldNotBeNull();
            _result.Method.ToLower().ShouldBe("get");
            _result.Scheme.ToLower().ShouldBe("http");
            _result.Host.ToLower().ShouldBe("www.test.com");
            _result.ToHttpRequestMessage().Content.ShouldBeNull();
        }
    }
}
