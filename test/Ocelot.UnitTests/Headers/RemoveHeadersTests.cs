using Ocelot.Middleware;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Headers
{
    public class RemoveHeadersTests
    {
        private List<Header> _headers;
        private readonly Ocelot.Headers.RemoveOutputHeaders _removeOutputHeaders;
        private Response _result;

        public RemoveHeadersTests()
        {
            _removeOutputHeaders = new Ocelot.Headers.RemoveOutputHeaders();
        }

        [Fact]
        public void should_remove_header()
        {
            var headers = new List<Header>()
            {
                new Header("Transfer-Encoding", new List<string> {"chunked"})
            };

            this.Given(x => x.GivenAHttpContext(headers))
                .When(x => x.WhenIRemoveTheHeaders())
                .Then(x => x.TheHeaderIsNoLongerInTheContext())
                .BDDfy();
        }

        private void GivenAHttpContext(List<Header> headers)
        {
            _headers = headers;
        }

        private void WhenIRemoveTheHeaders()
        {
            _result = _removeOutputHeaders.Remove(_headers);
        }

        private void TheHeaderIsNoLongerInTheContext()
        {
            _result.IsError.ShouldBeFalse();
            _headers.ShouldNotContain(x => x.Key == "Transfer-Encoding");
            _headers.ShouldNotContain(x => x.Key == "transfer-encoding");
        }
    }
}
