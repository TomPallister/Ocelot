using Ocelot.Request.Middleware;
using Shouldly;
using System;
using System.Net.Http;
using Xunit;

namespace Ocelot.UnitTests.Request
{
    public class DownstreamRequestTests
    {
        [Fact]
        public void should_have_question_mark_with_question_mark_prefixed()
        {
            var httpRequestMessage = new HttpRequestMessage();
            httpRequestMessage.RequestUri = new Uri("https://example.com/a?b=c");
            var downstreamRequest = new DownstreamRequest(httpRequestMessage);
            var result = downstreamRequest.ToHttpRequestMessage();
            result.RequestUri.Query.ShouldBe("?b=c");
        }
    }
}
