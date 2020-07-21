using Microsoft.AspNetCore.Http;
using Ocelot.Configuration;
using Ocelot.Headers;
using Ocelot.Responses;
using Shouldly;
using System.Collections.Generic;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Headers
{
    public class HttpContextRequestHeaderReplacerTests
    {
        private HttpContext _context;
        private List<HeaderFindAndReplace> _fAndRs;
        private HttpContextRequestHeaderReplacer _replacer;
        private Response _result;

        public HttpContextRequestHeaderReplacerTests()
        {
            _replacer = new HttpContextRequestHeaderReplacer();
        }

        [Fact]
        public void should_replace_headers()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add("test", "test");

            var fAndRs = new List<HeaderFindAndReplace>();
            fAndRs.Add(new HeaderFindAndReplace("test", "test", "chiken", 0));

            this.Given(x => GivenTheFollowingHttpRequest(context))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeadersAreReplaced())
                .BDDfy();
        }

        [Fact]
        public void should_not_replace_headers()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Add("test", "test");

            var fAndRs = new List<HeaderFindAndReplace>();

            this.Given(x => GivenTheFollowingHttpRequest(context))
                .And(x => GivenTheFollowingHeaderReplacements(fAndRs))
                .When(x => WhenICallTheReplacer())
                .Then(x => ThenTheHeadersAreNotReplaced())
                .BDDfy();
        }

        private void ThenTheHeadersAreNotReplaced()
        {
            _result.ShouldBeOfType<OkResponse>();
            foreach (var f in _fAndRs)
            {
                _context.Request.Headers.TryGetValue(f.Key, out var values);
                values[f.Index].ShouldBe("test");
            }
        }

        private void GivenTheFollowingHttpRequest(HttpContext context)
        {
            _context = context;
        }

        private void GivenTheFollowingHeaderReplacements(List<HeaderFindAndReplace> fAndRs)
        {
            _fAndRs = fAndRs;
        }

        private void WhenICallTheReplacer()
        {
            _result = _replacer.Replace(_context, _fAndRs);
        }

        private void ThenTheHeadersAreReplaced()
        {
            _result.ShouldBeOfType<OkResponse>();
            foreach (var f in _fAndRs)
            {
                _context.Request.Headers.TryGetValue(f.Key, out var values);
                values[f.Index].ShouldBe(f.Replace);
            }
        }
    }
}
