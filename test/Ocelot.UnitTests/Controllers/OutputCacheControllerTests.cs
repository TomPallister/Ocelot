using Microsoft.AspNetCore.Mvc;
using Moq;
using Ocelot.Cache;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.Controllers
{
    public class OutputCacheControllerTests
    {
        private OutputCacheController _controller;
        private Mock<IOcelotCache<CachedResponse>> _cache;
        private IActionResult _result;

        public OutputCacheControllerTests()
        {
            _cache = new Mock<IOcelotCache<CachedResponse>>();
            _controller = new OutputCacheController(_cache.Object);
        }

        [Fact]
        public void should_delete_key()
        {
            this.When(_ => WhenIDeleteTheKey("a"))
               .Then(_ => ThenTheKeyIsDeleted("a"))
               .BDDfy();
        }

        private void ThenTheKeyIsDeleted(string key)
        {
            _result.ShouldBeOfType<NoContentResult>();
            _cache
                .Verify(x => x.ClearRegion(key), Times.Once);
        }

        private void WhenIDeleteTheKey(string key)
        {
            _result = _controller.Delete(key);
        }
    }
}
